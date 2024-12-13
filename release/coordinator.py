import json
import os
from llm_model import create_story
from llm_model import generate_continuation, assert_valid_json, MAX_RETRIES

class Coordinator:
    def __init__(self):
        self.current_label = ""  
    
    # statically generate first N layers of storyline
    def initialize_storyline(self, IP, 
                             num_choices=4, choices_left=10, generation_threshold=2):
        self.IP = IP
        self.num_choices = num_choices
        self.choices_left = choices_left
        self.generation_threshold = generation_threshold
        # self.image_pipeline = initialize_image_model() # initialize image model pipeline from image_model.py

        # this should instead check firestore
        if not os.path.exists(f"data/{IP}.json"):
            self.story_json = create_story(IP, num_choices = self.num_choices, 
                                           choices_left = self.choices_left, 
                                           generation_threshold = self.generation_threshold)
            with open(f"data/{IP}.json", "w") as f:
                json.dump(self.story_json, f)
        else:
            with open(f"data/{IP}.json", "r") as f:
                self.story_json = json.load(f) 
        return self.story_json  
    
    # dynamically generate as needed
    # TODO: image generation should dynamically occur in this method; we can store image paths in JSONs, similar to text organization
    def continue_story(self, choice_id: str, current_label: str = None):
        # allows easy testing
        if current_label:
            self.current_label = current_label

        # next node exists
        if self.current_label + choice_id in self.story_json: 
            # update parameters
            self.current_label = self.current_label + choice_id
            self.choices_left -= 1
            return self.story_json[self.current_label]
        
        # dynamically generate next node
        else:
            context = self.story_json[""]
            current_path = int(self.current_label[0]) if self.current_label else 1
            current_node = self.story_json[self.current_label]

            for attempt in range(1, MAX_RETRIES + 1):
                continuation = generate_continuation(
                    context=context["plot_summary"] + context["branching_storylines"][current_path - 1]["story_line"],
                    previous_scenario=current_node["story_continuation"] if "story_continuation" in current_node else "",
                    previous_choice=current_node["choices"][int(choice_id)-1],
                    num_choices=self.num_choices,
                    choices_left=self.choices_left
                )

                if assert_valid_json(continuation):
                    break
                elif attempt == MAX_RETRIES:
                    print(f"Failed to parse JSON after {MAX_RETRIES} attempts during continue_story.")
                    return

            parsed_data = json.loads(continuation)
            self.story_json[self.current_label + choice_id] = parsed_data

            # update parameters
            self.current_label = self.current_label + choice_id
            self.choices_left -= 1

            with open(f"data/{self.IP}.json", "w") as f:
                json.dump(self.story_json, f)

            return self.story_json[self.current_label] # add to storage as well

if __name__ == '__main__':
    coord = Coordinator()
    # coord.initialize_storyline('League of Legends')
    coord.initialize_storyline('Dracula')
    
    print(coord.continue_story("1", "22"))
import json
import os
from release.llm_model import create_story
from release.llm_model import generate_continuation, assert_valid_json, MAX_RETRIES


class Coordinator:
    def __init__(self):
        pass
    
    # statically generate first N layers of storyline
    def initialize_storyline(self, IP, 
                             num_choices=4, choices_left=10, generation_threshold=2):
        self.IP = IP
        self.num_choices = num_choices
        self.choices_left = choices_left
        self.generation_threshold = generation_threshold

        if not os.path.exists(f"{IP}.json"):
            self.story_json = create_story(IP, num_choices = self.num_choices, 
                                           choices_left = self.choices_left, 
                                           generation_threshold = self.generation_threshold)
            with open(f"{IP}.json", "w") as f:
                json.dump(self.story_json, f)
        else:
            with open(f"{IP}.json", "r") as f:
                self.story_json = json.load(f) 
    
    # dynamically generate as needed
    def continue_story(self, current_label, next_label):
        if current_label + next_label in self.story_json: # next node exists
            return self.story_json[current_label + next_label]
        else:
            context = self.story_json[""]
            current_path = int(current_label[0])
            current_node = self.story_json[current_label]

            for attempt in range(1, MAX_RETRIES + 1):
                # dynamic generation for next story element
                continuation = generate_continuation(context = context["plot_summary"] + context["branching_storylines"][current_path - 1]["story_line"], 
                                                 previous_scenario = current_node["story_continuation"],
                                                 previous_choice = current_node["choices"][int(next_label)-1],
                                                 num_choices = self.num_choices,
                                                 choices_left = self.choices_left - len(current_label + next_label))
                if assert_valid_json(continuation):
                    break
                elif attempt == MAX_RETRIES:
                    print(f"Failed to parse JSON after {MAX_RETRIES} attempts.")
                    return
            
            parsed_data = json.loads(continuation)

            self.story_json[current_label + next_label] = parsed_data
            # can dump self.story_json at END!
            with open(f"{self.IP}.json", "w") as f:
                json.dump(self.story_json, f)

            return self.story_json[current_label + next_label]


if __name__ == '__main__':
    coord = Coordinator()
    coord.initialize_storyline('League of Legends')
    print(coord.continue_story("112", "4"))

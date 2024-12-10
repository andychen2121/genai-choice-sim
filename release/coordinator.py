import json
import os
from LLM_plot_gen import create_story


class Coordinator:
    def __init__(self):
        pass
    
    # statically generate first N layers of storyline
    def initialize_storyline(self, IP):
        if not os.path.exists(f"{IP}.json"):
            self.story_json = create_story(IP)
            with open(f"{IP}.json", "w") as f:
                json.dump(self.story_json, f)
        else:
            self.story_json = json.loads(f"{IP}.json")
    
    # TBD
    # gets next node; if DNE, dynamically generate next node for given story
    def continue_story(self, current_label, next_label):
        if current_label + next_label in self.story_json:
            return self.story_json[current_label + next_label]
        else:
            # TODO: dynamic generation + update json
            pass


if __name__ == '__main__':
    ex_json = create_story('League of Legends - Arcane')
    with open("example_data.json", "w") as f:
        json.dump(ex_json, f)
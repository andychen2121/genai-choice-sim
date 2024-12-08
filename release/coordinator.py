import json
from LLM_plot_gen import create_story

if __name__ == '__main__':
    ex_json = create_story('League of Legends - Arcane')
    with open("example_data.json", "w") as f:
        json.dump(ex_json, f)
import wikipediaapi
import json
from together import Together

MAX_RETRIES = 5

wiki_wiki = wikipediaapi.Wikipedia('MyProjectName (merlin@example.com)', 'en')
client = Together(api_key='0256f61cf32560dba2fb9c36573f46b473dbd1f2f0d95a731eeaf01b458e7bed')
all_jsons = {}

def get_plot(title):
    """
    Retrieves the plot summary of the given title from Wikipedia.
    """
    page = wiki_wiki.page(title)
    if not page.exists():
        print(f"Page '{title}' does not exist.")
        return None

    possible_sections = ['Plot', 'Synopsis', 'Summary', 'Plot summary', 'Story']
    plot_section = None
    for section_title in possible_sections:
        plot_section = page.section_by_title(section_title)
        if plot_section:
            break
    if plot_section:
        return plot_section.text
    else:
        print(f"No plot section found for '{title}'.")
        return None


def generate_continuation(context, previous_scenario=None, previous_choice=None, num_choices=4, choices_left=10):
    """
    Generates a continuation of the story based on the given context and an optional choice.
    choices_left details the amount of urgency left, to account for story pacing.
    """
    if choices_left == 0:
        return generate_final(context, previous_scenario, previous_choice)

    system_prompt = f"""
    Your job is to tell a continuation of the story based on what previously happened.
    You will be given the overall plot synopsis to help guide the story in that direction.
    The player has {choices_left} choice(s) left.

    Instructions:
    - Use simple, clear language that is easy to understand and avoids being overly descriptive.
    - Stay in third-person point of view.
    - Ensure each branching storyline offers meaningful player choices and consequences.
    - Stay concise, limiting each storyline description to 3-5 sentences.
    - The player has around 10 choices in total. If they have 5-7 left, ramp up the story, aiming for 3-4 choices left as the climax. Begin to aim for an interesting conclusion at 2-1 choices left.
    - All outputs should be in JSON format like so. Do not include anything else:
    {{
    "story_continuation": "[Your story continuation here]",
    "choices": [
        {{
        "choice": 1,
        "action": "[First possible action]"
        }},
        {{
        "choice": 2,
        "action": "[Second possible action]"
        }}
        ...
    ]
    }}
    """

    if previous_choice:
        prompt = f"""
        Write a continuation of the story that progresses the story forward. Include {num_choices} choices for the player to choose from given this generated situation.
        All outputs should be in JSON format. Do not include anything else.

        The following scenario has just occured:
        {previous_scenario}

        The player made the following choice:
        {previous_choice}

        Plot Summary: {context}
        """
    else:
        prompt = f"""
        We are at the start of our adventure. Write a continuation of the story that progresses the story forward. Include {num_choices} choices for the player to choose from given this generated situation.
        All outputs should be in JSON format.

        Plot Summary: {context}
        """
        
    try:
        output = client.chat.completions.create(
            model="meta-llama/Llama-3-70b-chat-hf",
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": prompt}
            ],
        )
        return output.choices[0].message.content
    except Exception as e:
        print(f"Error during generation: {e}")
        return None

def generate_final(context, previous_scenario, previous_choice):
    """
    Generates the conclusion of the story based on the given context and choice.
    """
    system_prompt = f"""
    Your job is to tell the conclusion of a story based on what previously happened.
    You will be given the overall plot synopsis to help guide the ending.

    Instructions:
    - Use simple, clear language that is easy to understand and avoids being overly descriptive.
    - Stay in third-person point of view.
    - Ensure each branching storyline offers meaningful player choices and consequences.
    - Stay concise, limiting each storyline description to 3-5 sentences.
    - All outputs should be in JSON format like so. Do not include anything else:
    {{
    "story_continuation": "[Your story continuation here]"
    }}
    """

    prompt = f"""
    Write a satisfying conclusion to the story.
    All outputs should be in JSON format. Do not include anything else.

    The following scenario has just occured:
    {previous_scenario}

    The player made the following choice:
    {previous_choice}

    Plot Summary: {context}
    """
        
    try:
        output = client.chat.completions.create(
            model="meta-llama/Llama-3-70b-chat-hf",
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": prompt}
            ],
        )
        return output.choices[0].message.content
    except Exception as e:
        print(f"Error during generation: {e}")
        return None


def initialize_story(IP, num_choices=4):
    """
    Generates the initial story framework for an interactive narrative based on a specific intellectual property (IP).
    This function adapts the original plot of the IP to create a new branching storyline with similar themes but fresh twists and engaging paths.

    Parameters:
    - `IP` (str): The intellectual property (e.g., movie, book, or game) to base the story on.
    - `num_choices` (int): The number of branching story paths to create. Default is 4.
    """

    system_prompt = f"""
    Your job is to adapt the story of {IP} and create a branching storyline that incorporates similar elements while introducing new twists and engaging paths for players.
    Instructions:
    - Use simple, clear language that is easy to understand and avoids being overly descriptive.
    - Ensure each branching storyline offers meaningful player choices and consequences.
    - Stay concise, limiting each storyline description to 3-5 sentences.
    - All outputs should be in JSON format like so. Do not include anything else:
    {{
      "plot_summary": "[Your initial plot summary here]",
      "branching_storylines": [
        {{
          "path": 1,
          "path_name": "Title of path"
          "story_line": "[Summary of first storyline]"
        }},
        {{
          "path": 2,
          "path_name": "Title of path"
          "story_line": "[Summary of second storyline]"
        }}
        ...
      ]
    }}
    """

    plot_prompt = f"""
    Generate a creative, original adaptation of {IP}'s plot, introducing similar themes and elements while creating 4 new, engaging branching storylines for players.
    All outputs should be in JSON format. Do not include anything else.

    {IP} Plot Summary: {get_plot(IP)}"""

    output = client.chat.completions.create(
        model="meta-llama/Llama-3-70b-chat-hf",
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": plot_prompt}
        ],
    )

    return output.choices[0].message.content


def assert_valid_json(json_str):
    """
    Attempts to parse a JSON string with retries upon failure.

    Parameters:
        json_str (str): The JSON string to parse.
        max_retries (int): Maximum number of retry attempts.

    Returns:
        True if valid JSON, False otherwise.
    """
    try:
        parsed_data = json.loads(json_str)
        return True
    except json.JSONDecodeError as e:
        return False


# Recursive Builder:
def populate_JSON(context,
                  previous_scenario, previous_choice,
                  label_idx,
                  generation_threshold,
                  num_choices=4, choices_left=10):
    """
    Recursively populates a JSON structure for a branching storyline.

    Parameters:
    - `context` (str): The overall storyline context to guide the creation of new continuations.
    - `previous_scenario` (str): The preceding storyline scenario from which new continuations branch.
    - `previous_choice` (str): The player's previous choice that led to the current scenario.
    - `label_idx` (str): Label for generated choice, used to id/navigate all_jsons.
    - `generation_threshold` (int): The number of branching layers left to create before recursive static generation stops. 
    - `num_choices` (int): The number of branching choices to create at each step. Default is 4.
    - `choices_left` (int): The remaining depth of choices before the story concludes.
    """

    if generation_threshold <= 0:
      return

    # Assert generated JSON is validly formatted given MAX_RETRIES
    for attempt in range(1, MAX_RETRIES + 1):
        output = generate_continuation(
            context,
            previous_scenario,
            previous_choice,
            num_choices=num_choices,
            choices_left=choices_left
        )

        if assert_valid_json(output):
          break
        elif attempt == MAX_RETRIES:
          print(f"Failed to parse JSON after {MAX_RETRIES} attempts.")
          return

    parsed_data = json.loads(output)
    all_jsons[label_idx] = parsed_data

    current_scenario = parsed_data.get("story_continuation")
    for id, choice in enumerate(parsed_data.get("choices")):
        current_choice = choice["action"]
        populate_JSON(context,
                      previous_scenario = current_scenario,
                      previous_choice = current_choice,
                      label_idx = label_idx + str(id + 1), # 1-indexed to match choice idxs
                      generation_threshold = generation_threshold - 1,
                      num_choices = num_choices,
                      choices_left = choices_left - 1)


# Only method we externally should interact with; other methods are abstracted
def create_story(IP, num_choices=4, choices_left=10, generation_threshold=2):
    """
    Creates a branching interactive story based on a specific intellectual property (IP), generating storylines recursively.

    Parameters:
    - `IP` (str): The intellectual property (e.g., movie, book, or game) to base the story on.
    - `num_choices` (int): The number of branching choices to create at each stage of the storyline. Default is 4.
    - `choices_left` (int): The total depth of choices before the story concludes.
    = `generation_threshold` (int): The number of branching layers to create before recursive static generation stops. 

    Returns:
        JSON containing all data generated.
    """
    # Prevent generating more than choices_left
    if generation_threshold > choices_left:
        raise ValueError("generation_threshold cannot be greater than choices_left.")

    # Assert generated JSON is validly formatted given MAX_RETRIES
    for attempt in range(1, MAX_RETRIES + 1):
        story = initialize_story(IP, num_choices)
        if assert_valid_json(story):
          break
        elif attempt == MAX_RETRIES:
          print(f"Failed to parse JSON after {MAX_RETRIES} attempts.")
          return

    story = json.loads(story)
    all_jsons[""] = story

    initial_context = story.get("plot_summary")

    for id, branch in enumerate(story.get("branching_storylines")):
        path_name = branch["path_name"] # TODO: add to final JSON + Unity
        story_line = branch["story_line"]

        # context for each branch is initial context + overall plot of specific branch
        context = initial_context + " " + story_line
        populate_JSON(context,
                      previous_scenario = None,
                      previous_choice = None,
                      label_idx = str(id + 1), # 1-indexed to match choice idxs
                      generation_threshold = generation_threshold,
                      num_choices = num_choices,
                      choices_left = choices_left)
    
    return all_jsons


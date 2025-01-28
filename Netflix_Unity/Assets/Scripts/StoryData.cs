using System.Collections.Generic;
using Newtonsoft.Json;

public class StoryData
{
    [JsonProperty("root")]
    public RootData Root { get; set; } // Represents the initial context

    [JsonIgnore] // Ignore during direct JSON deserialization
    public Dictionary<string, StoryNode> StoryNodes { get; set; } = new Dictionary<string, StoryNode>();
}

public class RootData
{
    [JsonProperty("plot_summary")]
    public string PlotSummary { get; set; }

    [JsonProperty("branching_storylines")]
    public List<BranchingStoryline> BranchingStorylines { get; set; }
}

public class BranchingStoryline
{
    [JsonProperty("path")]
    public int Path { get; set; }

    [JsonProperty("path_name")]
    public string PathName { get; set; }

    [JsonProperty("story_line")]
    public string StoryLine { get; set; }
}

public class StoryNode
{
    [JsonProperty("story_continuation")]
    public string StoryContinuation { get; set; }

    [JsonProperty("choices")]
    public List<Choice> Choices { get; set; }
}

public class Choice
{
    [JsonProperty("choice")]
    public int ChoiceId { get; set; } // Renamed to avoid conflict

    [JsonProperty("action")]
    public string Action { get; set; }
}
using System;

[Serializable]
public class StoryData
{
    public StoryNode[] storyNodes;
}

[Serializable]
public class StoryNode
{
    public int nodeIndex;
    public string nodeText;
    public string[] choices;
    public string backgroundImage;
    public int[] nextNodeIndices;
}
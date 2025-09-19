using System.Collections.Generic;

public static class DoorGlobalState
{
    // 以 doorID 为唯一键
    public static Dictionary<string, bool> IsOiled = new Dictionary<string, bool>();
    public static Dictionary<string, bool> IsOpen = new Dictionary<string, bool>();
}

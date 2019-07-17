using UnityEditor;
//http://answers.unity3d.com/questions/282959/set-inspector-lock-by-code.html
static class InspectorLock
{
   
    [MenuItem("Tools/Toggle Inspector Lock %q")] // Ctrl + q
    static void ToggleInspectorLock()
    {
        ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }
}

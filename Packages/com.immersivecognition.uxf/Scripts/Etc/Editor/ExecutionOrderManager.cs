using UnityEditor;

[InitializeOnLoad]
public class ExecutionOrderManager : Editor
{
    static ExecutionOrderManager()
    {
        // Get the name of the script we want to change it's execution order
        string fileSaver = typeof(UXF.FileSaver).Name;

        // Iterate through all scripts (Might be a better way to do this?)
        foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
        {
            // If found our script
            if (monoScript.name == fileSaver)
            {
                // And it's not at the execution time we want already
                // (Without this we will get stuck in an infinite loop)
                if (MonoImporter.GetExecutionOrder(monoScript) != 1000)
                {
                    MonoImporter.SetExecutionOrder(monoScript, 1000);
                }
                break;
            }
        }
    }
}
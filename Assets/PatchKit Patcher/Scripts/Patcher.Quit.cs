using UnityEngine;

public partial class Patcher
{
    //TODO: Make it private after moving to Legacy
    public void Quit()
    {
        if (State.Kind != PatcherStateKind.Idle &&
            State.Kind != PatcherStateKind.StartingApp)
        {
            return;
        }

        ModifyState(x: () => State.Kind = PatcherStateKind.Quitting);

#if UNITY_EDITOR
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        else
#endif
        {
            Application.Quit();
        }
    }
}
using UnityEngine.SceneManagement;

namespace PROJECTNAME.Interfaces
{
public interface ISceneChangeHandler
{
	void OnSceneChange(Scene scene, LoadSceneMode mode);
}
}
using UnityEngine.SceneManagement;

namespace PixelCiv.Interfaces
{
public interface ISceneChangeHandler
{
	void OnSceneChange(Scene scene, LoadSceneMode mode);
}
}
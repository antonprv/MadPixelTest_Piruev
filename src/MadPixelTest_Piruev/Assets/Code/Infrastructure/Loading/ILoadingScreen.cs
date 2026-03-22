using Cysharp.Threading.Tasks;

namespace BagFight.Infrastructure.Loading
{
  public interface ILoadingScreen
  {
    void Show();
    void Hide();
    void SetProgress(float value); // 0..1
    UniTask ShowAsync();
    UniTask HideAsync();
  }
}

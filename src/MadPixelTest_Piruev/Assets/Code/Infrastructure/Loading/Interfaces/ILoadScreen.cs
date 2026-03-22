// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.Loading
{
  public interface ILoadScreen
  {
    void Show();
    void Hide();
    void SetProgress(float value); // 0..1
    UniTask ShowAsync();
    UniTask HideAsync();
  }
}

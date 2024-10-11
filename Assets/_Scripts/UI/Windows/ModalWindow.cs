using UnityEngine;
using Michsky.UI.Shift;
using Cysharp.Threading.Tasks;


public class ModalWindow : MonoBehaviour
{
    public bool sharpAnimations = false;
    private BlurManager _blurManager;
    private Animator _mWindowAnimator;
    private bool _isOn;

    public void Awake()
    {
        _mWindowAnimator = gameObject.GetComponent<Animator>();
        _blurManager = GetComponentInParent<BlurManager>();
    }
    
    public void ModalWindowIn()
    {
        gameObject.SetActive(true);
        _blurManager.BlurInAnim();

        if (_isOn) return;
        _isOn = true;

        if (sharpAnimations == false)
            _mWindowAnimator.CrossFade("Window In", 0.1f);
        else
            _mWindowAnimator.Play("Window In");
    }

    public void ModalWindowOut()
    {
        DisableWindow().Forget();
        _blurManager.BlurOutAnim();

        if (! _isOn) return;

        if (sharpAnimations == false)
            _mWindowAnimator.CrossFade("Window Out", 0.1f);
        else
            _mWindowAnimator.Play("Window Out");

        _isOn = false;
    }

    private async UniTaskVoid DisableWindow()
    {
        await UniTask.Delay(500);
        gameObject.SetActive(false);
    }
}

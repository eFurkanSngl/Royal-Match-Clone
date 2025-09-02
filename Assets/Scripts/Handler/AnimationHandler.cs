using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    public void DestroyAnim(Transform target , float duration , Ease ease, System.Action onComplete = null)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return;
        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOScale(Vector3.zero, duration).SetEase(ease));
        var sr = target.GetComponent<SpriteRenderer>();
        if (sr != null && sr.gameObject != null && sr.enabled)
        {
            seq.Join(sr.DOFade(0f, duration).SetEase(ease));
        }
        seq.OnComplete(() => {
            if (target != null && target.gameObject != null)
                onComplete?.Invoke();
        });
    }
    public void SwapAnim(Transform from ,Vector3 target , float duration , Ease ease, System.Action onComplete = null)
    {
        if (from == null || !from.gameObject.activeInHierarchy) return;
        Sequence seq = DOTween.Sequence();
        seq.Append(from.DOMove(target,duration).SetEase(ease));
        seq.OnComplete(() => {
            if (from != null && from.gameObject != null)
                onComplete?.Invoke();
        });
    }

    public void PowerUpAnim(Transform transform, Vector3 scale , float duration, Ease ease)
    {
        if (transform == null || !transform.gameObject.activeInHierarchy) return;
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(scale, duration).SetEase(ease));
        Debug.Log(" is worked");
    }

    public IEnumerator GatherMatchedTileRoutine(List<Tile> matchedTiles, Tile originTile, float duration)
    {
        Sequence seq = DOTween.Sequence();
        foreach (Tile match in matchedTiles)
        {
            if (match != null && match.gameObject != null && originTile != null && originTile.gameObject != null)
            {
                var matchTransform = match.transform;
                var originTransform = originTile.transform;
                if (matchTransform != null && matchTransform.gameObject.activeInHierarchy && originTransform != null && originTransform.gameObject.activeInHierarchy)
                {
                    seq.Join(matchTransform.DOMove(originTransform.position, duration)
                        .SetEase(Ease.InQuad));
                    seq.Join(matchTransform.DOScale(0.3f, duration));
                }
            }
        }
        yield return seq.WaitForCompletion(); // Sabit wait yerine animasyon gerçekten bittiğinde devam et
    }
}

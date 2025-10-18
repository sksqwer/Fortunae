using UnityEngine;

public class PocketTrigger : MonoBehaviour
{
    [Header("[ 포켓 넘버 설정 ]")]
    [SerializeField] public int pocketNumber;
    
    // Scene 뷰에서 포켓 위치 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
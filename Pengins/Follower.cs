using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Follower : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField]
    private float m_FollowDistance = 2.5f;

    [SerializeField]
    private float m_StopDistance = 1.5f;

    [Header("Animation")]
    [SerializeField]
    private string m_VerticalID = "Vert";

    [SerializeField]
    private string m_StateID = "State";

    [SerializeField]
    private float m_AnimationSmooth = 8f;

    [Header("Game")]
    [SerializeField]
    private float m_DisableY = -3f;

    private Transform m_Target;

    private NavMeshAgent m_Agent;
    private Animator m_Animator;

    private bool m_IsCollected;
    private bool m_IsFollowing = true;

    private float m_SpeedBlend;

    public bool IsCollected => m_IsCollected;

    private void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();

        m_Agent.stoppingDistance = m_StopDistance;
        m_Agent.autoBraking = true;
    }

    private void Update()
    {
        if (!m_IsCollected)
        {
            return;
        }

        if (transform.position.y < m_DisableY)
        {
            return;
        }

        if (!m_IsFollowing)
        {
            m_Agent.isStopped = true;
            Animate(0f);
            return;
        }

        if (m_Target == null)
        {
            Animate(0f);
            return;
        }

        m_Agent.isStopped = false;

        float distance =
            Vector3.Distance(
                transform.position,
                m_Target.position);

        if (distance > m_FollowDistance)
        {
            m_Agent.SetDestination(
                m_Target.position);
        }

        Animate(m_Agent.velocity.magnitude);
    }

    private void Animate(float speed)
    {
        float targetBlend = 0f;

        if (m_Agent.speed > 0.01f)
        {
            targetBlend =
                Mathf.Clamp01(
                    speed / m_Agent.speed);
        }

        m_SpeedBlend = Mathf.Lerp(
            m_SpeedBlend,
            targetBlend,
            m_AnimationSmooth * Time.deltaTime);

        m_Animator.SetFloat(
            m_VerticalID,
            m_SpeedBlend,
            0.15f,
            Time.deltaTime);

        m_Animator.SetFloat(
            m_StateID,
            m_SpeedBlend > 0.1f ? 1f : 0f,
            0.15f,
            Time.deltaTime);
    }

    public void Collect()
    {
        if (m_IsCollected)
        {
            return;
        }

        m_IsCollected = true;

        FollowerManager.Instance.AddFollower(this);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPenguin();
        }

        Sequence seq = DOTween.Sequence();

        seq.Append(
            transform.DOJump(
                transform.position,
                0.5f,
                1,
                0.4f));

        seq.Join(
            transform.DOPunchScale(
                Vector3.one * 0.3f,
                0.4f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_IsCollected)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        Collect();
    }

    public void SetTarget(Transform target)
    {
        m_Target = target;
    }

    public void ToggleFollow()
    {
        m_IsFollowing = !m_IsFollowing;

        transform.DOKill();

        if (m_IsFollowing)
        {
            transform.DOPunchScale(
                Vector3.one * 0.2f,
                0.2f);
        }
        else
        {
            transform.DOShakeRotation(
                0.2f,
                15f);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class FollowerManager : MonoBehaviour
{
    public static FollowerManager Instance { get; private set; }

    [SerializeField]
    private Transform m_Player;

    private readonly List<Follower> m_Followers = new();

    public int Count => m_Followers.Count;

    private void Awake()
    {
        Instance = this;
    }

    public void AddFollower(Follower follower)
    {
        if (follower == null)
        {
            return;
        }

        if (m_Followers.Contains(follower))
        {
            return;
        }

        Transform target = m_Player;

        if (m_Followers.Count > 0)
        {
            target = m_Followers[m_Followers.Count - 1].transform;
        }

        follower.SetTarget(target);

        m_Followers.Add(follower);
    }

    public void RemoveFollower(Follower follower)
    {
        if (!m_Followers.Remove(follower))
        {
            return;
        }

        RebuildFormation();
    }

    private void RebuildFormation()
    {
        Transform currentTarget = m_Player;

        foreach (var follower in m_Followers)
        {
            if (follower == null)
            {
                continue;
            }

            follower.SetTarget(currentTarget);
            currentTarget = follower.transform;
        }
    }

    public void ToggleAllFollowers()
    {
        foreach (var follower in m_Followers)
        {
            if (follower == null)
            {
                continue;
            }

            follower.ToggleFollow();
        }
    }
}
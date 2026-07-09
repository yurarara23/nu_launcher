using System;
using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public class CharactorMover : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        private float m_WalkSpeed = 4f;

        [SerializeField]
        private float m_RunSpeed = 12f;

        [SerializeField, Range(0f, 360f)]
        private float m_RotateSpeed = 360f;

        [SerializeField]
        private Space m_Space = Space.Self;

        [Header("Jump")]
        [SerializeField]
        private float m_JumpHeight = 1.5f;

        [SerializeField]
        private float m_Gravity = -20f;

        [SerializeField]
        private float m_GroundedVelocity = -2f;

        [Tooltip("足場から離れた直後でもジャンプできる時間")]
        [SerializeField]
        private float m_CoyoteTime = 0.12f;

        [Tooltip("着地直前に押したジャンプ入力を保持する時間")]
        [SerializeField]
        private float m_JumpBufferTime = 0.15f;

        [Header("Animator")]
        [SerializeField]
        private string m_VerticalID = "Vert";

        [SerializeField]
        private string m_StateID = "State";

        [SerializeField]
        private string m_GroundedID = "Grounded";

        [SerializeField]
        private string m_VerticalVelocityID = "VerticalVelocity";

        [SerializeField]
        private string m_JumpID = "Jump";

        [SerializeField]
        private LookWeight m_LookWeight = new(1f, 0.3f, 0.7f, 1f);

        private Transform m_Transform;
        private CharacterController m_Controller;
        private Animator m_Animator;

        private MovementHandler m_Movement;
        private AnimationHandler m_Animation;

        private Vector2 m_Axis;
        private Vector3 m_Target;

        private bool m_IsRun;
        private bool m_IsMoving;

        // 次回のUpdateで消費するジャンプ入力
        private bool m_JumpRequested;

        // ジャンプボタンの押しっぱなしを検出するために使用
        private bool m_WasJumpHeld;

        public Vector2 Axis => m_Axis;
        public Vector3 Target => m_Target;
        public bool IsRun => m_IsRun;

        private void OnValidate()
        {
            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
            m_RunSpeed = Mathf.Max(m_RunSpeed, m_WalkSpeed);

            m_JumpHeight = Mathf.Max(m_JumpHeight, 0f);
            m_Gravity = Mathf.Min(m_Gravity, -0.01f);
            m_GroundedVelocity = Mathf.Min(m_GroundedVelocity, -0.01f);

            m_CoyoteTime = Mathf.Max(m_CoyoteTime, 0f);
            m_JumpBufferTime = Mathf.Max(m_JumpBufferTime, 0f);

            m_Movement?.SetStats(
                m_WalkSpeed / 3.6f,
                m_RunSpeed / 3.6f,
                m_RotateSpeed,
                m_JumpHeight,
                m_Gravity,
                m_GroundedVelocity,
                m_CoyoteTime,
                m_JumpBufferTime,
                m_Space
            );
        }

        private void Awake()
        {
            m_Transform = transform;
            m_Controller = GetComponent<CharacterController>();
            m_Animator = GetComponent<Animator>();

            m_Movement = new MovementHandler(
                m_Controller,
                m_Transform,
                m_WalkSpeed / 3.6f,
                m_RunSpeed / 3.6f,
                m_RotateSpeed,
                m_JumpHeight,
                m_Gravity,
                m_GroundedVelocity,
                m_CoyoteTime,
                m_JumpBufferTime,
                m_Space
            );

            m_Animation = new AnimationHandler(
                m_Animator,
                m_VerticalID,
                m_StateID,
                m_GroundedID,
                m_VerticalVelocityID,
                m_JumpID
            );
        }

        private void Update()
        {
            // 入力を一度だけ消費する
            bool jumpRequested = m_JumpRequested;
            m_JumpRequested = false;

            m_Movement.Move(
                Time.deltaTime,
                in m_Axis,
                in m_Target,
                m_IsRun,
                m_IsMoving,
                jumpRequested,
                out var animAxis,
                out var isGrounded,
                out var verticalVelocity,
                out var didJump
            );

            m_Animation.Animate(
                in animAxis,
                m_IsRun ? 1f : 0f,
                isGrounded,
                verticalVelocity,
                didJump,
                Time.deltaTime
            );
        }

        private void OnAnimatorIK()
        {
            m_Animation.AnimateIK(in m_Target, m_LookWeight);
        }

        /// <summary>
        /// 移動入力を設定します。
        /// isJumpにはボタンの押下状態を渡せます。
        /// </summary>
        public void SetInput(
            in Vector2 axis,
            in Vector3 target,
            in bool isRun,
            in bool isJump)
        {
            m_Axis = axis;
            m_Target = target;
            m_IsRun = isRun;

            // 押した瞬間だけジャンプ要求を発行
            if (isJump && !m_WasJumpHeld)
            {
                m_JumpRequested = true;
            }

            m_WasJumpHeld = isJump;

            if (m_Axis.sqrMagnitude < Mathf.Epsilon)
            {
                m_Axis = Vector2.zero;
                m_IsMoving = false;
            }
            else
            {
                m_Axis = Vector2.ClampMagnitude(m_Axis, 1f);
                m_IsMoving = true;
            }
        }

        /// <summary>
        /// InputActionのperformedなどから直接呼ぶ場合に使用します。
        /// </summary>
        public void RequestJump()
        {
            m_JumpRequested = true;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            float surfaceAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (surfaceAngle <= m_Controller.slopeLimit + 0.1f)
            {
                m_Movement.SetSurface(hit.normal);
            }
        }

        [Serializable]
        private struct LookWeight
        {
            public float weight;
            public float body;
            public float head;
            public float eyes;

            public LookWeight(
                float weight,
                float body,
                float head,
                float eyes)
            {
                this.weight = weight;
                this.body = body;
                this.head = head;
                this.eyes = eyes;
            }
        }

        #region Handlers

        private class MovementHandler
        {
            private readonly CharacterController m_Controller;
            private readonly Transform m_Transform;

            private float m_WalkSpeed;
            private float m_RunSpeed;
            private float m_RotateSpeed;

            private float m_JumpHeight;
            private float m_Gravity;
            private float m_GroundedVelocity;

            private float m_CoyoteTime;
            private float m_JumpBufferTime;

            private Space m_Space;

            private Vector3 m_Normal = Vector3.up;
            private Vector3 m_LastForward;

            private float m_VerticalVelocity;
            private float m_CoyoteTimer;
            private float m_JumpBufferTimer;

            public MovementHandler(
                CharacterController controller,
                Transform transform,
                float walkSpeed,
                float runSpeed,
                float rotateSpeed,
                float jumpHeight,
                float gravity,
                float groundedVelocity,
                float coyoteTime,
                float jumpBufferTime,
                Space space)
            {
                m_Controller = controller;
                m_Transform = transform;

                m_LastForward = transform.forward;

                SetStats(
                    walkSpeed,
                    runSpeed,
                    rotateSpeed,
                    jumpHeight,
                    gravity,
                    groundedVelocity,
                    coyoteTime,
                    jumpBufferTime,
                    space
                );
            }

            public void SetStats(
                float walkSpeed,
                float runSpeed,
                float rotateSpeed,
                float jumpHeight,
                float gravity,
                float groundedVelocity,
                float coyoteTime,
                float jumpBufferTime,
                Space space)
            {
                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;

                m_JumpHeight = jumpHeight;
                m_Gravity = gravity;
                m_GroundedVelocity = groundedVelocity;

                m_CoyoteTime = coyoteTime;
                m_JumpBufferTime = jumpBufferTime;

                m_Space = space;
            }

            public void SetSurface(in Vector3 normal)
            {
                m_Normal = normal;
            }

            public void Move(
                float deltaTime,
                in Vector2 axis,
                in Vector3 target,
                bool isRun,
                bool isMoving,
                bool jumpRequested,
                out Vector2 animAxis,
                out bool isGrounded,
                out float verticalVelocity,
                out bool didJump)
            {
                Vector3 cameraLook = target - m_Transform.position;
                cameraLook.y = 0f;

                if (cameraLook.sqrMagnitude < Mathf.Epsilon)
                {
                    cameraLook = m_Transform.forward;
                }
                else
                {
                    cameraLook.Normalize();
                }

                ConvertMovement(in axis, in cameraLook, out var movement);

                if (movement.sqrMagnitude > 0.001f)
                {
                    m_LastForward = movement.normalized;
                }

                UpdateJumpBuffer(deltaTime, jumpRequested);
                UpdateGroundState(deltaTime);

                didJump = TryJump();

                ApplyGravity(deltaTime, didJump);
                Displace(deltaTime, in movement, isRun);

                Turn(in m_LastForward, isMoving, deltaTime);
                GenAnimationAxis(in movement, out animAxis);

                isGrounded = m_Controller.isGrounded;

                if (isGrounded && m_VerticalVelocity < 0f)
                {
                    m_VerticalVelocity = m_GroundedVelocity;
                }

                verticalVelocity = m_VerticalVelocity;
            }

            private void UpdateJumpBuffer(
                float deltaTime,
                bool jumpRequested)
            {
                if (jumpRequested)
                {
                    m_JumpBufferTimer = m_JumpBufferTime;
                }
                else
                {
                    m_JumpBufferTimer = Mathf.Max(
                        m_JumpBufferTimer - deltaTime,
                        0f
                    );
                }
            }

            private void UpdateGroundState(float deltaTime)
            {
                if (m_Controller.isGrounded)
                {
                    m_CoyoteTimer = m_CoyoteTime;

                    if (m_VerticalVelocity < 0f)
                    {
                        m_VerticalVelocity = m_GroundedVelocity;
                    }
                }
                else
                {
                    m_CoyoteTimer = Mathf.Max(
                        m_CoyoteTimer - deltaTime,
                        0f
                    );

                    // 空中では斜面の法線を引き継がない
                    m_Normal = Vector3.up;
                }
            }

            private bool TryJump()
            {
                bool canJump =
                    m_JumpBufferTimer > 0f &&
                    m_CoyoteTimer > 0f;

                if (!canJump)
                {
                    return false;
                }

                // v = √(2gh)
                m_VerticalVelocity = Mathf.Sqrt(
                    m_JumpHeight * -2f * m_Gravity
                );

                // 同じ入力で複数回ジャンプしないように消費
                m_JumpBufferTimer = 0f;
                m_CoyoteTimer = 0f;

                return true;
            }

            private void ApplyGravity(
                float deltaTime,
                bool didJump)
            {
                if (!m_Controller.isGrounded || didJump)
                {
                    m_VerticalVelocity += m_Gravity * deltaTime;
                }
            }

            private void ConvertMovement(
                in Vector2 axis,
                in Vector3 targetForward,
                out Vector3 movement)
            {
                Vector3 forward;
                Vector3 right;

                if (m_Space == Space.Self)
                {
                    forward = new Vector3(
                        targetForward.x,
                        0f,
                        targetForward.z
                    ).normalized;

                    right = Vector3.Cross(
                        Vector3.up,
                        forward
                    ).normalized;
                }
                else
                {
                    forward = Vector3.forward;
                    right = Vector3.right;
                }

                movement = axis.x * right + axis.y * forward;

                // 接地中の斜面に沿わせる
                if (m_Controller.isGrounded)
                {
                    movement = Vector3.ProjectOnPlane(
                        movement,
                        m_Normal
                    );
                }

                movement = Vector3.ClampMagnitude(movement, 1f);
            }

            private void Displace(
                float deltaTime,
                in Vector3 movement,
                bool isRun)
            {
                float speed = isRun
                    ? m_RunSpeed
                    : m_WalkSpeed;

                Vector3 velocity = movement * speed;
                velocity.y = m_VerticalVelocity;

                m_Controller.Move(velocity * deltaTime);
            }

            private void Turn(
                in Vector3 targetForward,
                bool isMoving,
                float deltaTime)
            {
                if (!isMoving)
                {
                    return;
                }

                Vector3 forward = Vector3.ProjectOnPlane(
                    targetForward,
                    Vector3.up
                );

                if (forward.sqrMagnitude < Mathf.Epsilon)
                {
                    return;
                }

                Quaternion targetRotation = Quaternion.LookRotation(
                    forward.normalized,
                    Vector3.up
                );

                m_Transform.rotation = Quaternion.RotateTowards(
                    m_Transform.rotation,
                    targetRotation,
                    m_RotateSpeed * deltaTime
                );
            }

            private void GenAnimationAxis(
                in Vector3 movement,
                out Vector2 animAxis)
            {
                if (m_Space == Space.Self)
                {
                    animAxis = new Vector2(
                        Vector3.Dot(movement, m_Transform.right),
                        Vector3.Dot(movement, m_Transform.forward)
                    );
                }
                else
                {
                    animAxis = new Vector2(
                        Vector3.Dot(movement, Vector3.right),
                        Vector3.Dot(movement, Vector3.forward)
                    );
                }
            }
        }

        private class AnimationHandler
        {
            private readonly Animator m_Animator;

            private readonly int m_VerticalID;
            private readonly int m_StateID;
            private readonly int m_GroundedID;
            private readonly int m_VerticalVelocityID;
            private readonly int m_JumpID;

            private readonly float k_InputFlow = 4.5f;

            private float m_FlowState;
            private Vector2 m_FlowAxis;

            public AnimationHandler(
                Animator animator,
                string verticalID,
                string stateID,
                string groundedID,
                string verticalVelocityID,
                string jumpID)
            {
                m_Animator = animator;

                m_VerticalID = Animator.StringToHash(verticalID);
                m_StateID = Animator.StringToHash(stateID);
                m_GroundedID = Animator.StringToHash(groundedID);
                m_VerticalVelocityID =
                    Animator.StringToHash(verticalVelocityID);
                m_JumpID = Animator.StringToHash(jumpID);
            }

            public void Animate(
                in Vector2 axis,
                float state,
                bool isGrounded,
                float verticalVelocity,
                bool didJump,
                float deltaTime)
            {
                m_FlowAxis = Vector2.MoveTowards(
                    m_FlowAxis,
                    axis,
                    k_InputFlow * deltaTime
                );

                m_FlowState = Mathf.MoveTowards(
                    m_FlowState,
                    state,
                    k_InputFlow * deltaTime
                );

                m_Animator.SetFloat(
                    m_VerticalID,
                    m_FlowAxis.magnitude
                );

                m_Animator.SetFloat(
                    m_StateID,
                    Mathf.Clamp01(m_FlowState)
                );

                m_Animator.SetBool(
                    m_GroundedID,
                    isGrounded
                );

                m_Animator.SetFloat(
                    m_VerticalVelocityID,
                    verticalVelocity
                );

                if (didJump)
                {
                    m_Animator.SetTrigger(m_JumpID);
                }
            }

            public void AnimateIK(
                in Vector3 target,
                in LookWeight lookWeight)
            {
                m_Animator.SetLookAtPosition(target);

                m_Animator.SetLookAtWeight(
                    lookWeight.weight,
                    lookWeight.body,
                    lookWeight.head,
                    lookWeight.eyes
                );
            }
        }

        #endregion
    }
}
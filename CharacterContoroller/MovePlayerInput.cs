using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    [RequireComponent(typeof(CharactorMover))]
    public class MovePlayerInput : MonoBehaviour
    {
        private enum InputDeviceType
        {
            KeyboardMouse,
            Gamepad
        }
        
        [Header("使用する入力機器")]
        [SerializeField] private InputDeviceType m_InputDeviceType;
        
        // 👈 【追加】ゲームパッド用の回転感度（インスペクターから調整してください）
        [Header("Gamepad Settings")]
        [SerializeField] private float m_GamepadSensitivity = 200f;
        
        [Header("Camera")]
        [SerializeField] private PlayerCamera m_Camera;
        
        [Header("Character Inputs")]
        [SerializeField] private InputAction m_MoveAction;
        [SerializeField] private InputAction m_LookAction;
        [SerializeField] private InputAction m_JumpAction;
        [SerializeField] private InputAction m_RunAction;

        private CharactorMover m_Mover;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;

        private Vector3 m_Target;
        private Vector2 m_MouseDelta;

        private void Awake()
        {
            m_Mover = GetComponent<CharactorMover>();
        }

        private void OnEnable()
        {
            m_MoveAction?.Enable();
            m_LookAction?.Enable();
            m_JumpAction?.Enable();
            m_RunAction?.Enable();
        }

        private void OnDisable()
        {
            m_MoveAction?.Disable();
            m_LookAction?.Disable();
            m_JumpAction?.Disable();
            m_RunAction?.Disable();
        }

        private void Update()
        {
            GatherInput();
            SetInput();
        }

        public void GatherInput()
        {
            m_Axis = m_MoveAction.ReadValue<Vector2>();
            m_IsRun = m_RunAction.IsPressed();
            m_IsJump = m_JumpAction.WasPressedThisFrame();

            m_Target = (m_Camera == null)
                ? Vector3.zero
                : m_Camera.Target;

            // 一旦、現在の入力値をそのまま取得
            m_MouseDelta = m_LookAction.ReadValue<Vector2>();

            // 👈 【追加】ゲームパッドが選択されている場合は、時間と感度を掛けて調整する
            if (m_InputDeviceType == InputDeviceType.Gamepad)
            {
                m_MouseDelta = m_MouseDelta * m_GamepadSensitivity * Time.deltaTime;
            }
        }

        public void BindMover(CharactorMover mover)
        {
            m_Mover = mover;
        }

        public void SetInput()
        {
            if (m_Mover != null)
            {
                m_Mover.SetInput(
                    in m_Axis,
                    in m_Target,
                    in m_IsRun,
                    m_IsJump);
            }

            if (m_Camera != null)
            {
                m_Camera.SetInput(
                    in m_MouseDelta,
                    0f);
            }
        }
    }
}
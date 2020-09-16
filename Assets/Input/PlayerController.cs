// GENERATED AUTOMATICALLY FROM 'Assets/Input/PlayerController.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerController : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerController()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerController"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""ec5cc72c-31f6-423e-80be-6acae711ed66"",
            ""actions"": [
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""554b1de5-051c-484e-9f10-51832b56341f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""JumpCancel"",
                    ""type"": ""Button"",
                    ""id"": ""c531ac21-490d-41a0-bd58-b211fb876ab9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Run"",
                    ""type"": ""Button"",
                    ""id"": ""a77c817f-6280-42de-a9d5-18bc7148a5d2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RunKickOff"",
                    ""type"": ""Button"",
                    ""id"": ""1357f26e-d977-4830-880d-ea80666c9be9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""MultiTap""
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""1db7a0d3-b1c5-4f2b-ab5d-898f7894a44a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""MultiTap""
                },
                {
                    ""name"": ""Permeation"",
                    ""type"": ""Button"",
                    ""id"": ""cf7f6aa5-fd5c-4b22-9349-2a0a8f3eeefa"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CameraManualTilt"",
                    ""type"": ""Value"",
                    ""id"": ""f313189f-99b9-4b63-87bc-501ee392182c"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CameraChangeSetting"",
                    ""type"": ""Button"",
                    ""id"": ""1cd3cd6c-d1ec-4e65-afbc-71c47ffd9431"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CameraResetManualTilt"",
                    ""type"": ""Button"",
                    ""id"": ""1580beff-3ea7-4942-b54e-6a6d3c522f47"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""1e66adf2-18a4-4869-9cde-f115c6f4464e"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Standard"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""6f7f8b6a-3c0f-4869-a038-42867b470153"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""8071039c-c81c-4f35-af32-4c4d37b3aa7d"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""63242d00-7f33-45f7-9c2f-ddf8e5fc0427"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""836f345a-c5a9-4e2b-a925-00da4cc860ea"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": ""Standard"",
                    ""action"": ""JumpCancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3093db90-358f-4c8e-9b05-36ba80c134f2"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""Standard"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""41ad8fe6-64e2-4bcd-88b3-b8e705b12125"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraManualTilt"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""fc516886-2e3b-4918-a2f0-93e23eddbfd3"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Standard"",
                    ""action"": ""CameraManualTilt"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""b9c3a8d1-d5a8-4c73-aabb-f44046363465"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Standard"",
                    ""action"": ""CameraManualTilt"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""25e3d62f-91a3-40c6-8a2c-bb10c6c5d6dc"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Standard"",
                    ""action"": ""CameraManualTilt"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""883ea299-97f1-434d-bc18-a3c871ceee41"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Standard"",
                    ""action"": ""CameraManualTilt"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""bfeb04c3-790f-4697-938c-a03d9763fee5"",
                    ""path"": ""<Keyboard>/rightAlt"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": ""Standard"",
                    ""action"": ""CameraChangeSetting"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6963ffd5-7fdd-4a9b-90f0-a713f7298a8a"",
                    ""path"": ""<Keyboard>/rightAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Standard"",
                    ""action"": ""CameraResetManualTilt"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Standard"",
            ""bindingGroup"": ""Standard"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
        m_Player_JumpCancel = m_Player.FindAction("JumpCancel", throwIfNotFound: true);
        m_Player_Run = m_Player.FindAction("Run", throwIfNotFound: true);
        m_Player_RunKickOff = m_Player.FindAction("RunKickOff", throwIfNotFound: true);
        m_Player_Pause = m_Player.FindAction("Pause", throwIfNotFound: true);
        m_Player_Permeation = m_Player.FindAction("Permeation", throwIfNotFound: true);
        m_Player_CameraManualTilt = m_Player.FindAction("CameraManualTilt", throwIfNotFound: true);
        m_Player_CameraChangeSetting = m_Player.FindAction("CameraChangeSetting", throwIfNotFound: true);
        m_Player_CameraResetManualTilt = m_Player.FindAction("CameraResetManualTilt", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_JumpCancel;
    private readonly InputAction m_Player_Run;
    private readonly InputAction m_Player_RunKickOff;
    private readonly InputAction m_Player_Pause;
    private readonly InputAction m_Player_Permeation;
    private readonly InputAction m_Player_CameraManualTilt;
    private readonly InputAction m_Player_CameraChangeSetting;
    private readonly InputAction m_Player_CameraResetManualTilt;
    public struct PlayerActions
    {
        private @PlayerController m_Wrapper;
        public PlayerActions(@PlayerController wrapper) { m_Wrapper = wrapper; }
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        public InputAction @JumpCancel => m_Wrapper.m_Player_JumpCancel;
        public InputAction @Run => m_Wrapper.m_Player_Run;
        public InputAction @RunKickOff => m_Wrapper.m_Player_RunKickOff;
        public InputAction @Pause => m_Wrapper.m_Player_Pause;
        public InputAction @Permeation => m_Wrapper.m_Player_Permeation;
        public InputAction @CameraManualTilt => m_Wrapper.m_Player_CameraManualTilt;
        public InputAction @CameraChangeSetting => m_Wrapper.m_Player_CameraChangeSetting;
        public InputAction @CameraResetManualTilt => m_Wrapper.m_Player_CameraResetManualTilt;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @JumpCancel.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpCancel;
                @JumpCancel.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpCancel;
                @JumpCancel.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJumpCancel;
                @Run.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @Run.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @Run.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @RunKickOff.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRunKickOff;
                @RunKickOff.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRunKickOff;
                @RunKickOff.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRunKickOff;
                @Pause.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPause;
                @Pause.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPause;
                @Pause.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPause;
                @Permeation.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPermeation;
                @Permeation.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPermeation;
                @Permeation.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPermeation;
                @CameraManualTilt.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraManualTilt;
                @CameraManualTilt.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraManualTilt;
                @CameraManualTilt.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraManualTilt;
                @CameraChangeSetting.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraChangeSetting;
                @CameraChangeSetting.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraChangeSetting;
                @CameraChangeSetting.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraChangeSetting;
                @CameraResetManualTilt.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraResetManualTilt;
                @CameraResetManualTilt.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraResetManualTilt;
                @CameraResetManualTilt.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraResetManualTilt;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @JumpCancel.started += instance.OnJumpCancel;
                @JumpCancel.performed += instance.OnJumpCancel;
                @JumpCancel.canceled += instance.OnJumpCancel;
                @Run.started += instance.OnRun;
                @Run.performed += instance.OnRun;
                @Run.canceled += instance.OnRun;
                @RunKickOff.started += instance.OnRunKickOff;
                @RunKickOff.performed += instance.OnRunKickOff;
                @RunKickOff.canceled += instance.OnRunKickOff;
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @Permeation.started += instance.OnPermeation;
                @Permeation.performed += instance.OnPermeation;
                @Permeation.canceled += instance.OnPermeation;
                @CameraManualTilt.started += instance.OnCameraManualTilt;
                @CameraManualTilt.performed += instance.OnCameraManualTilt;
                @CameraManualTilt.canceled += instance.OnCameraManualTilt;
                @CameraChangeSetting.started += instance.OnCameraChangeSetting;
                @CameraChangeSetting.performed += instance.OnCameraChangeSetting;
                @CameraChangeSetting.canceled += instance.OnCameraChangeSetting;
                @CameraResetManualTilt.started += instance.OnCameraResetManualTilt;
                @CameraResetManualTilt.performed += instance.OnCameraResetManualTilt;
                @CameraResetManualTilt.canceled += instance.OnCameraResetManualTilt;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    private int m_StandardSchemeIndex = -1;
    public InputControlScheme StandardScheme
    {
        get
        {
            if (m_StandardSchemeIndex == -1) m_StandardSchemeIndex = asset.FindControlSchemeIndex("Standard");
            return asset.controlSchemes[m_StandardSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnJump(InputAction.CallbackContext context);
        void OnJumpCancel(InputAction.CallbackContext context);
        void OnRun(InputAction.CallbackContext context);
        void OnRunKickOff(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnPermeation(InputAction.CallbackContext context);
        void OnCameraManualTilt(InputAction.CallbackContext context);
        void OnCameraChangeSetting(InputAction.CallbackContext context);
        void OnCameraResetManualTilt(InputAction.CallbackContext context);
    }
}

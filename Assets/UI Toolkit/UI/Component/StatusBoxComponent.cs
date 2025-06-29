using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace UIToolkit
{
    public class StatusBoxComponent : VisualElement
    {
        private static class ClassNames
        {
            public static string StatusBoxContainer = "statusBox__container";
            public static string StatusBoxPlayerIconContainer = "statusBox__characterIconContainer";
            public static string StatusBoxStatusContainer = "statusBox__statusContainer";
            public static string StatusBoxUpperContainer = "statusBox__upperContainer";
            public static string StatusBoxLowerContainer = "statusBox__lowerContainer";
            public static string StatusBoxCharacterIcon = "statusBox__characterIcon";
            public static string StatusBoxStatusIcon = "statusBox__statusIcon";
            public static string StatusBoxLabel = "statusBox__label";
            public static string BasicLabel = "label";
        }

        public new class UxmlFactory : UxmlFactory<StatusBoxComponent, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(visualElement, bag, cc);
                var statusBoxComponent = (StatusBoxComponent)visualElement;
            }
        }

        private Parameter _parameter;
        private VisualElement _leftContainer;
        private VisualElement _rightContainer;
        private VisualElement _upperContainer;
        private VisualElement _lowerContainer;
        private VisualElement _playerIcon;
        private VisualElement _hitPointIcon;
        private VisualElement _manaPointIcon;
        private VisualElement _powerIcon;
        private Label _nameLabel;
        private Label _hitPointLabel;
        private Label _manaPointLabel;
        private Label _powerLabel;

        public Parameter Parameter
        {
            get => _parameter;
            set => _parameter = value;
        }

        public StatusBoxComponent()
        {
            AddToClassList(ClassNames.StatusBoxContainer);
            InitializeContainers();
            InitializeElements();
            PutContainersAndElements();
            SetDefaultStyle();
        }

        private void InitializeContainers()
        {
            _leftContainer = new VisualElement();
            _leftContainer.name = "LeftContainer";
            _leftContainer.AddToClassList(ClassNames.StatusBoxPlayerIconContainer);


            _rightContainer = new VisualElement();
            _rightContainer.name = "RightContainer";
            _rightContainer.AddToClassList(ClassNames.StatusBoxStatusContainer);
            _upperContainer = new VisualElement();
            _upperContainer.name = "UpperContainer";
            _upperContainer.AddToClassList(ClassNames.StatusBoxUpperContainer);
            _lowerContainer = new VisualElement();
            _lowerContainer.name = "LowerContainer";
            _lowerContainer.AddToClassList(ClassNames.StatusBoxLowerContainer);
        }

        private void InitializeElements()
        {
            _playerIcon = new VisualElement();
            _playerIcon.name = "PlayerIcon";
            _hitPointIcon = new VisualElement();
            _hitPointIcon.name = "HitPointIcon";
            _manaPointIcon = new VisualElement();
            _manaPointIcon.name = "ManaPointIcon";
            _powerIcon = new VisualElement();
            _powerIcon.name = "PowerIcon";
            _nameLabel = new Label();
            _nameLabel.name = "NameLabel";
            _hitPointLabel = new Label();
            _hitPointLabel.name = "HitPointLabel";
            _manaPointLabel = new Label();
            _manaPointLabel.name = "ManaPointLabel";
            _powerLabel = new Label();
            _powerLabel.name = "PowerLabel";

            _playerIcon.AddToClassList(ClassNames.StatusBoxCharacterIcon);
            _hitPointIcon.AddToClassList(ClassNames.StatusBoxStatusIcon);
            _manaPointIcon.AddToClassList(ClassNames.StatusBoxStatusIcon);
            _powerIcon.AddToClassList(ClassNames.StatusBoxStatusIcon);
            _nameLabel.AddToClassList(ClassNames.StatusBoxLabel);
            _nameLabel.AddToClassList(ClassNames.BasicLabel);
            _hitPointLabel.AddToClassList(ClassNames.StatusBoxLabel);
            _hitPointLabel.AddToClassList(ClassNames.BasicLabel);
            _manaPointLabel.AddToClassList(ClassNames.StatusBoxLabel);
            _manaPointLabel.AddToClassList(ClassNames.BasicLabel);
            _powerLabel.AddToClassList(ClassNames.StatusBoxLabel);
            _powerLabel.AddToClassList(ClassNames.BasicLabel);
        }

        private void PutContainersAndElements()
        {
            Add(_leftContainer);
            Add(_rightContainer);

            _leftContainer.Add(_playerIcon);

            _rightContainer.Add(_upperContainer);
            _upperContainer.Add(_nameLabel);

            _rightContainer.Add(_lowerContainer);
            _lowerContainer.Add(_hitPointIcon);
            _lowerContainer.Add(_hitPointLabel);
            _lowerContainer.Add(_manaPointIcon);
            _lowerContainer.Add(_manaPointLabel);
            _lowerContainer.Add(_powerIcon);
            _lowerContainer.Add(_powerLabel);
        }

        private void SetDefaultStyle()
        {
            _nameLabel.text = "Jonson";
            _hitPointLabel.text = "10";
            _manaPointLabel.text = "10";
            _powerLabel.text = "3";
        }

        public void UpdateStatuBoxElments(Entity entity, Sprite heartIcon, Sprite manaIcon, Sprite powerIcon)
        {
            var parameter = entity.BaseParameter;
            _parameter = parameter;
            _playerIcon.style.backgroundImage = Addressables.LoadAssetAsync<Sprite>(entity.FieldSpriteAssetReference).WaitForCompletion().texture;
            _hitPointIcon.style.backgroundImage = heartIcon.texture;
            _manaPointIcon.style.backgroundImage = manaIcon.texture;
            _powerIcon.style.backgroundImage = powerIcon.texture;
            _nameLabel.text = parameter.Name;
            _hitPointLabel.text = entity.Hp.ToString();
            _manaPointLabel.text = entity.Mp.ToString();
            _powerLabel.text = entity.BaseParameter.Power.ToString();
        }
    }
}

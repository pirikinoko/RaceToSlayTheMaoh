using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit
{
    public class IndicatorBarComponent : VisualElement
    {
        public enum BarType
        {
            Health,
            Mana,
        }

        private static class ClassNames
        {
            public static string IndicatorBarContainer = "indicator-bar__container";
            public static string IndicatorBar = "indicator-bar";
            public static string IndicatorBarLabel = "indicator-bar__label";
        }

        private string GetIconClassNameByBarType(BarType barType)
        {
            return barType switch
            {
                BarType.Health => "indicator-bar__icon__health",
                BarType.Mana => "indicator-bar__icon__mana",
                _ => string.Empty,
            };
        }

        private string GetProglessClassNameByBarType(BarType barType)
        {
            return barType switch
            {
                BarType.Health => "indicator-bar__progress__health",
                BarType.Mana => "indicator-bar__progress__mana",
                _ => string.Empty,
            };
        }

        public new class UxmlFactory : UxmlFactory<IndicatorBarComponent, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlEnumAttributeDescription<BarType> _barType = new UxmlEnumAttributeDescription<BarType>
            { name = "barType", defaultValue = BarType.Health };

            readonly UxmlIntAttributeDescription _currentValue = new UxmlIntAttributeDescription
            { name = "currentValue", defaultValue = 50 };

            readonly UxmlIntAttributeDescription _minimumValue = new UxmlIntAttributeDescription
            { name = "minimumValue", defaultValue = 0 };

            readonly UxmlIntAttributeDescription _maximumValue = new UxmlIntAttributeDescription
            { name = "maximumValue", defaultValue = 100 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var indicatorBar = (IndicatorBarComponent)ve;

                indicatorBar.IndicatorBarType = _barType.GetValueFromBag(bag, cc);
                indicatorBar.CurrentValue = _currentValue.GetValueFromBag(bag, cc);
                indicatorBar.MinimumValue = _minimumValue.GetValueFromBag(bag, cc);
                indicatorBar.MaximumValue = _maximumValue.GetValueFromBag(bag, cc);
            }
        }

        // データが正しくリンクされ、ビルダーに表示されるようにするために、定義された Traits と同じ名前のプロパティを持つ必要があります
        private BarType _barType;
        private int _currentValue;
        private int _minimumValue;
        private int _maximumValue;
        private readonly Label _valueStat;
        private VisualElement _indicatorBar;
        private VisualElement _progress;
        private VisualElement _icon;

        public BarType IndicatorBarType
        {
            get => _barType;
            set
            {
                if (_barType == value)
                {
                    return;
                }

                // 以前のクラスを削除
                if (_barType != null)
                {
                    _icon.RemoveFromClassList(GetIconClassNameByBarType(_barType));
                    _progress.RemoveFromClassList(GetProglessClassNameByBarType(_barType));
                }

                // 新しいクラスを追加
                _barType = value;
                _icon.AddToClassList(GetIconClassNameByBarType(_barType));
                _progress.AddToClassList(GetProglessClassNameByBarType(_barType));
            }
        }

        public int CurrentValue
        {
            get => _currentValue;
            set
            {
                if (value == _currentValue)
                {
                    return;
                }

                _currentValue = value;
                SetValue(_currentValue, _maximumValue);
            }
        }
        public int MinimumValue
        {
            get => _minimumValue;
            set => _minimumValue = value;
        }

        public int MaximumValue
        {
            get => _maximumValue;
            set
            {
                if (value == _maximumValue)
                {
                    return;
                }

                _maximumValue = value;
                SetValue(_currentValue, _maximumValue);
            }
        }

        public IndicatorBarComponent()
        {
            AddToClassList(ClassNames.IndicatorBarContainer);

            _icon = new VisualElement { name = "IndicatorBarIcon" };
            _icon.AddToClassList(GetIconClassNameByBarType(_barType));
            Add(_icon);

            _indicatorBar = new VisualElement { name = "IndicatorBar" };
            _indicatorBar.AddToClassList(ClassNames.IndicatorBar);
            Add(_indicatorBar);

            _progress = new VisualElement { name = "IndicatorBarProgress" };
            _progress.AddToClassList(GetProglessClassNameByBarType(_barType));
            _indicatorBar.Add(_progress);

            _valueStat = new Label() { name = "IndicatorBarStat" };
            _valueStat.AddToClassList(ClassNames.IndicatorBarLabel);
            _indicatorBar.Add(_valueStat);
        }

        private void SetValue(int currentValue, int maxValue)
        {
            _valueStat.text = $"{currentValue}";
            if (maxValue > 0)
            {
                float w = Mathf.Clamp((float)currentValue / maxValue * 100, 0f, 100f);
                _progress.style.width = new StyleLength(Length.Percent(w));
            }
        }
    }
}

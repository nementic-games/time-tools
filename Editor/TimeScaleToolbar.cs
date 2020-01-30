// Copyright (c) 2019 Nementic Games GmbH. All Rights Reserved.
// Author: Chris Yarbrough 

namespace Nementic
{
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class TimeScaleToolbar
    {
        private float maxValue = 10f;
        private VisualElement root;

        public void BuildToolbar(VisualElement root)
        {
            this.root = root;

            var slider = root.Q<Slider>();
            slider.lowValue = 0f;
            slider.highValue = maxValue;
            float savedTimeScale = EditorPrefs.GetFloat("Nementic/TimeScaleToolbar/TimeScale", 1f);
            slider.value = (float)Math.Round(savedTimeScale, 2);

            var timeField = root.Q<FloatField>("Time");
            timeField.maxLength = 3;
            timeField.value = slider.value;
            timeField.RegisterValueChangedCallback(evt =>
            {
                float value = (float)Math.Round(Mathf.Max(0, evt.newValue), 2);
                gameTimeScale = value;
                slider.value = value;
            });

            slider.RegisterValueChangedCallback(evt =>
            {
                float value = (float)Math.Round(Mathf.Max(0, evt.newValue), 2);
                gameTimeScale = value;
                timeField.SetValueWithoutNotify(value);
                EditorPrefs.SetFloat("Nementic/TimeScaleToolbar/TimeScale", slider.value);
            });

            var maxTimeField = root.Q<FloatField>("MaxTime");
            maxTimeField.maxLength = 3;
            maxTimeField.value = slider.highValue;
            maxTimeField.RegisterValueChangedCallback(evt =>
            {
                slider.highValue = evt.newValue;
                maxValue = evt.newValue;
            });

            var button = root.Q<Button>();
            var icon = button.Children().First();
            button.clickable.clicked += () =>
            {
                gameTimeScale = 1f;
                slider.value = 1f;
                timeField.SetValueWithoutNotify(1f);
            };
            button.Add(icon);
        }

        public float gameTimeScale
        {
            set
            {
                GameTime.defaultTimeScale = value;
                GameTime.timeScale = value;
            }
        }

        public bool visible
        {
            set => root.visible = value;
        }
    }

    public sealed class TimeScaleGameViewToolbar
    {
        private const string menuName = "Nementic/Utility/Show Time Scale Toolbar";

        private static TimeScaleToolbar toolbar;

        [InitializeOnLoadMethod]
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.delayCall += () =>
            {
                bool toolbarEnabled = EditorPrefs.GetBool(menuName, false);
                Menu.SetChecked(menuName, toolbarEnabled);
                SetToolbarEnabled(toolbarEnabled);
            };
        }

        [MenuItem(menuName)]
        public static void ToggleToolbar()
        {
            bool enabled = EditorPrefs.GetBool(menuName, false);
            enabled = !enabled;
            SetToolbarEnabled(enabled);
            EditorPrefs.SetBool(menuName, enabled);
            Menu.SetChecked(menuName, enabled);
        }

        [MenuItem(menuName, validate = true)]
        private static bool ToggleToolbar_Validate()
        {
            bool toolbarEnabled = EditorPrefs.GetBool(menuName, false);
            Menu.SetChecked(menuName, toolbarEnabled);
            return true;
        }

        private static void SetToolbarEnabled(bool enabled)
        {
            if (enabled)
            {
                if (toolbar == null)
                {
                    System.Reflection.Assembly assembly = typeof(EditorWindow).Assembly;
                    Type type = assembly.GetType("UnityEditor.GameView");
                    var gameView = EditorWindow.GetWindow(type);

                    toolbar = new TimeScaleToolbar();
                    AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.nementic.time-tools/Editor/TimeScaleToolbar.uxml").CloneTree(gameView.rootVisualElement);
                    toolbar.BuildToolbar(gameView.rootVisualElement);
                    toolbar.gameTimeScale = gameView.rootVisualElement.Q<Slider>().value;
                }
                else
                    toolbar.visible = true;
            }
            else if (toolbar != null)
            {
                toolbar.visible = false;
            }
        }
    }
}

﻿using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UObject = UnityEngine.Object;

namespace GBG.EditorMessages.Editor
{
    public class MessageElement : VisualElement
    {
        public Label LineNumberLabel { get; }
        public Image TypeImage { get; }
        public Label TimestampLabel { get; }
        public Label TimestampSeparatorLabel { get; }
        public Label ContentLabel { get; }
        public Image ContextImage { get; }
        public Image CustomDataImage { get; }

        public Message Message { get; private set; }
        public int LineNumber { get; private set; } = -1;
        public int LineNumberLabelWidth { get; set; } = -1;
        public bool ShowTimestamp { get; set; } = true;

        public event Action<Message> WantsToProcessCustomData;


        public MessageElement()
        {
            style.flexDirection = FlexDirection.Row;
            style.paddingLeft = 4;
            style.paddingRight = 4;
            style.minWidth = 100;


            LineNumberLabel = new Label
            {
                style =
                {
                    flexShrink = 0,
                    marginRight = 2,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleRight,
                    unityFontDefinition = new StyleFontDefinition(ResCache.GetMonospaceFontAsset()),
                }
            };
            Add(LineNumberLabel);

            TypeImage = new Image
            {
                style =
                {
                    alignSelf = Align.Center,
                    minWidth = 16,
                    maxWidth = 16,
                    minHeight = 16,
                    maxHeight = 16,
                }
            };
            Add(TypeImage);

            TimestampLabel = new Label
            {
                style =
                {
                    paddingRight = 0,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontDefinition = new StyleFontDefinition(ResCache.GetMonospaceFontAsset()),
                }
            };
            Add(TimestampLabel);

            TimestampSeparatorLabel = new Label
            {
                text = "|",
                style =
                {
                    paddingLeft = 0,
                    paddingRight = 4,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    //unityFontDefinition = new StyleFontDefinition(ResCache.GetMonospaceFontAsset()),
                }
            };
            Add(TimestampSeparatorLabel);

            ContentLabel = new Label
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontDefinition = new StyleFontDefinition(ResCache.GetMonospaceFontAsset()),
                }
            };
            Add(ContentLabel);

            ContextImage = new Image
            {
                tooltip = "This message has context.",
                style =
                {
                    alignSelf = Align.Center,
                    minWidth = 16,
                    maxWidth = 16,
                    minHeight = 16,
                    maxHeight = 16,
                }
            };
            Add(ContextImage);

            CustomDataImage = new Image
            {
                tooltip = "This message has custom data.",
                style =
                {
                    alignSelf = Align.Center,
                    minWidth = 16,
                    maxWidth = 16,
                    minHeight = 16,
                    maxHeight = 16,
                }
            };
            Add(CustomDataImage);

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<ContextClickEvent>(OnContextClick);
        }

        public void SetMessage(Message message, int lineNumber, int lineNumberLabelWidth = -1)
        {
            Assert.IsTrue(message != null);

            Message = message;
            LineNumber = lineNumber;
            LineNumberLabelWidth = lineNumberLabelWidth;
            TypeImage.image = ResCache.GetMessageTypeIcon(message.Type);
            ContentLabel.text = message.Content;

            UpdateLineNumberLabel();
            UpdateTimestampLabel();
            UpdateContextImage();
            UpdateCustomDataImage();
        }

        private void UpdateLineNumberLabel()
        {
            if (LineNumber < 0)
            {
                LineNumberLabel.style.display = DisplayStyle.None;
                return;
            }

            LineNumberLabel.text = LineNumber.ToString();
            LineNumberLabel.style.width = LineNumberLabelWidth > 0 ? LineNumberLabelWidth : StyleKeyword.Auto;
            LineNumberLabel.style.display = DisplayStyle.Flex;
        }

        private void UpdateTimestampLabel()
        {
            if (!ShowTimestamp)
            {
                TimestampLabel.style.display = DisplayStyle.None;
                TimestampSeparatorLabel.style.display = DisplayStyle.None;
                return;
            }

            TimestampLabel.text = new DateTime(Message.Timestamp).ToString(CultureInfo.CurrentCulture);
            TimestampLabel.style.display = DisplayStyle.Flex;
            TimestampSeparatorLabel.style.display = DisplayStyle.Flex;
        }

        private void UpdateContextImage()
        {
            if (string.IsNullOrEmpty(Message.Context))
            {
                ContextImage.style.display = DisplayStyle.None;
                return;
            }

            ContextImage.image = ResCache.GetContextIcon();
            ContextImage.style.display = DisplayStyle.Flex;
        }

        private void UpdateCustomDataImage()
        {
            if (string.IsNullOrEmpty(Message.CustomData))
            {
                CustomDataImage.style.display = DisplayStyle.None;
                return;
            }

            CustomDataImage.image = ResCache.GetCustomDataIcon();
            CustomDataImage.style.display = DisplayStyle.Flex;
        }

        private void OnClick(ClickEvent evt)
        {
            if (evt.clickCount == 1)
            {
                UObject context = Message?.GetUnityContextObject();
                if (context)
                {
                    EditorGUIUtility.PingObject(context);
                }
            }
            else if (evt.clickCount == 2 && !string.IsNullOrEmpty(Message?.CustomData))
            {
                if (WantsToProcessCustomData != null)
                {
                    WantsToProcessCustomData(Message);
                }
                else
                {
                    Debug.LogError($"Custom data handler is not registered: {Message}", Message.GetUnityContextObject());
                }
            }
        }

        private void OnContextClick(ContextClickEvent evt)
        {
            GenericMenu menu = new GenericMenu();

            // Copy Content
            menu.AddItem(new GUIContent("Copy Content"), false, () => EditorGUIUtility.systemCopyBuffer = Message.Content);

            // Copy Context
            if (!string.IsNullOrEmpty(Message.Context))
            {
                menu.AddItem(new GUIContent("Copy Context"), false, () => EditorGUIUtility.systemCopyBuffer = Message.Context);
            }

            // Copy Custom Data
            if (!string.IsNullOrEmpty(Message.CustomData))
            {
                menu.AddItem(new GUIContent("Copy Custom Data"), false, () => EditorGUIUtility.systemCopyBuffer = Message.CustomData);
            }

            menu.ShowAsContext();
        }
    }
}

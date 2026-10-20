using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SPACS.Dialogs
{
    public class DialogPanelControllerGeneric : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        protected DialogPanel playerPanel;
        [SerializeField]
        protected DialogPanel npcPanel;

        [SerializeField]
        protected bool initializeOnStart = false;
        //Typewrite effect settings
        [SerializeField]
        protected float charactersPerSecond = 20f;
        [SerializeField]
        protected float interpunctuationDelay = 0.5f;
        protected TypewriterEffect typewriterEffect;
        protected bool typeWriteActive;
        [SerializeField]
        protected bool enableSkip;
        [SerializeField]
        protected bool quickSkip;
        [SerializeField]
        [Min(1)]
        protected int skipSpeedup = 5;

        protected DialogSystem dialogSystemInUse = default;
        protected DialogPanel currentDialogPanel;

        private bool dialogStarted = false;

        [System.Serializable]
        public class DialogPanel
        {
            [Tooltip("Gameobject containing the whole dialog panel")]
            public GameObject panelObject = default;
            [Tooltip("Option to show nickname text")]
            public bool showNickname = false;
            [Tooltip("Nickname bg object")]
            public GameObject nicknameBg = default;
            [Tooltip("Text component that will display the character name")]
            public TMP_Text nicknameText = default;
            [Tooltip("Text component that will display the dialog text")]
            public TMP_Text dialogText = default;
            [Tooltip("Option to show avatar image")]
            public bool showAvatarContainer = false;
            [Tooltip("List of Gameobjects containing the buttons needed for dialog choices")]
            public GameObject[] choiceButtonGroups = default;

            // This will store a reference to the text component of each choice button, so that 
            // the button labels can be customized for each dialog.
            public List<List<TMP_Text>> choiceButtonLabels = default;
            [HideInInspector]
            public bool isPlayer;
        }

        public DialogPanel PlayerPanel { get => playerPanel; }
        public DialogPanel NpcPanel { get => npcPanel; }

        public Action<Transform> OnNewDialogStart;

        /// <summary>
        /// Makes the dialog system in use step on to the next dialog along the dialogue path.
        /// This is supposed to be called by the buttons on the dialog panel.
        /// </summary>
        /// <param name="choice">The dialog option to follow in the path.</param>
        public void ContinueDialog(int choice)
        {
            //Manage skip and all possible cases with next dialog and typewrite effect
            if (enableSkip)
            {
                if (typewriterEffect.CurrentlySkipping)
                    return;

                if (!typewriterEffect.ReadyForNewText)
                    typewriterEffect.Skip(currentDialogPanel.dialogText);
                else
                    dialogSystemInUse.ContinueDialog(choice);
            }
            else
            {
                if (typeWriteActive)
                {
                    if (!typewriterEffect.ReadyForNewText)
                        return;
                    else
                        dialogSystemInUse.ContinueDialog(choice);
                }
                else
                    dialogSystemInUse.ContinueDialog(choice);
            }
        }

        /// <summary>
        /// This can be called to stop a dialog session, even if the last node in a 
        /// dialog path hasn't been reached.
        /// </summary>
        public void CancelDialog()
        {
            dialogSystemInUse.CancelDialog();
            dialogSystemInUse = null;
        }


        public void OnDialogChanged(DialogSystem dialogSystem)
        {
            DialogNode currentDialog = dialogSystem.CurrentDialog;
            // If a dialog is in progress, enables the panel and uses the dialog data 
            // to fill the panel fields.
            if (currentDialog != null)
            {
                //Check if there are another dialogSystem active
                if (dialogSystemInUse != null && dialogSystemInUse != dialogSystem)
                {
                    CancelDialog();
                    dialogStarted = false;
                }

                if (currentDialogPanel != null)
                    currentDialogPanel.panelObject.SetActive(false);

                if (!dialogStarted)
                {
                    dialogStarted = true;
                    OnNewDialogStart?.Invoke(dialogSystem.DialogPanelPos);
                }

                currentDialogPanel = currentDialog.npcDialogPanel ? npcPanel : playerPanel;

                dialogSystemInUse = dialogSystem;
                currentDialogPanel.panelObject.SetActive(true);
                SetChoiceButtons(currentDialog, currentDialogPanel);
                SetNicknameText(currentDialog.Character, currentDialog.CharacterNameSide, currentDialogPanel);
                SetAvatar(currentDialog.Avatar, currentDialog.CharacterAvatarSide, currentDialogPanel);
                SetDialogText(currentDialog.Dialog, currentDialogPanel);
            }
            // The system is moving to the next dialog: keep the panel and the dialog
            // session, the next dialog's change comes right after.
            else if (dialogSystem.IsAdvancing)
            {
                return;
            }
            // if the there is no dialog in progress, disables the dialog panel.
            else
            {
                if (currentDialogPanel != null)
                    currentDialogPanel.panelObject.SetActive(false);
                dialogSystemInUse = null;
                dialogStarted = false;
            }
        }

        private void Start()
        {
            if (initializeOnStart)
            {
                Init();
            }
        }

        public void Init()
        {
            playerPanel.isPlayer = true;
            npcPanel.isPlayer = false;
            SetUpPanels(playerPanel);
            SetUpPanels(npcPanel);

            typewriterEffect = GetComponent<TypewriterEffect>();
            if (typewriterEffect == null)
            {
                Debug.LogError("TypeWriteEffect component is missing!");
                return;
            }

            typeWriteActive = charactersPerSecond > 0;
            if (typeWriteActive)
            {
                typewriterEffect.Setup(charactersPerSecond, interpunctuationDelay, quickSkip, skipSpeedup);
                playerPanel.dialogText.maxVisibleCharacters = 0;
                npcPanel.dialogText.maxVisibleCharacters = 0;
            }
        }

        protected virtual void SetUpPanels(DialogPanel panel)
        {
            if (!panel.panelObject)
                return;

            panel.panelObject.SetActive(false);

            panel.choiceButtonLabels = new List<List<TMP_Text>>();
            // Fetches a reference to the TextMeshPro Text component of every choice button 
            // label and stores it in choiceButtonLabels. 
            for (int i = 0; i < panel.choiceButtonGroups.Length; i++)
            {
                List<TMP_Text> componentList =
                    panel.choiceButtonGroups[i].GetComponentsInChildren<TMP_Text>(true).ToList();

                // Sends a warning if not enough buttons are found as children of each button group.
                if ((i == 0 && componentList.Count < 1) || (i > 0 && componentList.Count < i))
                {
                    Debug.LogWarning("Not enough Button type components found in button group " + i +
                        " (" + panel.choiceButtonGroups[i].name + ").");
                }
                // Always added, so that choiceButtonLabels[n] stays the group of n options:
                // SetChoiceButtons skips the labels a short group does not have.
                panel.choiceButtonLabels.Add(componentList);
            }
        }

        private static void SetChoiceLabel(DialogPanel panel, int group, int index, string label)
        {
            if (panel.choiceButtonLabels == null || group >= panel.choiceButtonLabels.Count)
                return;
            List<TMP_Text> labels = panel.choiceButtonLabels[group];
            if (index < labels.Count && labels[index] != null)
                labels[index].text = label;
        }

        public virtual void SetSettings(float _charactersPerSecond, float _interpunctuationDelay, bool _enableSkip, bool _quickSkip, int _skipSpeedup,
            bool _showPlayerNickname, bool _showNpcNickname, bool _showPlayerAvatarContainer, bool _showNpcAvatarContainer, bool useOtherNickname = false, bool useOtherAvatar = false)
        { }

        /// <summary>
        /// Enables the button group needed by the dialog element, and sets all button labels.
        /// </summary>
        /// <param name="dialog"></param>
        protected virtual void SetChoiceButtons(DialogNode dialog, DialogPanel currentPanel)
        {
            foreach (GameObject buttonGroup in currentPanel.choiceButtonGroups)
            {
                buttonGroup.SetActive(false);
            }

            int optionCount = dialog.OptionCount;
            if (optionCount >= currentPanel.choiceButtonGroups.Length)
            {
                Debug.LogWarning($"Dialog '{dialog.Dialog}' has {optionCount} options, but the panel " +
                    $"'{currentPanel.panelObject.name}' has no button group for that many.");
                return;
            }
            currentPanel.choiceButtonGroups[optionCount].SetActive(true);
            switch (optionCount)
            {
                case 0:
                    SetChoiceLabel(currentPanel, 0, 0, dialog.Option1Label);
                    break;
                case 1:
                    SetChoiceLabel(currentPanel, 1, 0, dialog.Option1Label);
                    break;
                case 2:
                    SetChoiceLabel(currentPanel, 2, 0, dialog.Option1Label);
                    SetChoiceLabel(currentPanel, 2, 1, dialog.Option2Label);
                    break;
                case 3:
                    SetChoiceLabel(currentPanel, 3, 0, dialog.Option1Label);
                    SetChoiceLabel(currentPanel, 3, 1, dialog.Option2Label);
                    SetChoiceLabel(currentPanel, 3, 2, dialog.Option3Label);
                    break;
                case 4:
                    SetChoiceLabel(currentPanel, 4, 0, dialog.Option1Label);
                    SetChoiceLabel(currentPanel, 4, 1, dialog.Option2Label);
                    SetChoiceLabel(currentPanel, 4, 2, dialog.Option3Label);
                    SetChoiceLabel(currentPanel, 4, 3, dialog.Option4Label);
                    break;
            }
        }

        protected virtual void SetNicknameText(string titleId, DialogNode.LayoutElementSide characterNameSide, DialogPanel currentPanel)
        {
            if (!currentPanel.showNickname || string.IsNullOrEmpty(titleId))
                currentPanel.nicknameBg.SetActive(false);
            else
            {
                currentPanel.nicknameBg.SetActive(true);
                currentPanel.nicknameText.text = titleId;
            }
        }

        protected virtual void SetDialogText(string dialogId, DialogPanel currentPanel)
        {
            currentPanel.dialogText.text = dialogId;
            currentPanel.dialogText.ForceMeshUpdate();

            //Typewirte Effect logic
            if (typeWriteActive)
                typewriterEffect.PrepareForNewText(currentPanel.dialogText);
        }

        protected virtual void SetAvatar(Sprite texture, DialogNode.LayoutElementSide characterAvatarSide, DialogPanel currentPanel) { }
    }
}

// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can Cleric tab-target combat, slow cast cadence, mana pressure, and med-break recovery make the silence between pulls feel intentional rather than empty?
// Date: 2026-04-26

using UnityEngine;
using UnityEngine.UIElements;

namespace Gravenspire.Prototypes.CombatFeel
{
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        [SerializeField] private CadenceKnobs knobs = new();

        private CombatLoop loop;
        private UIDocument document;
        private Label stateLabel;
        private Label playerLabel;
        private Label targetLabel;
        private Label previewLabel;
        private Label castLabel;
        private Label metricsLabel;
        private Label logLabel;
        private VisualElement healthFill;
        private VisualElement manaFill;
        private VisualElement targetHealthFill;
        private VisualElement castFill;
        private Button pullButton;
        private Button smiteButton;
        private Button healButton;
        private Button medButton;
        private Button stopButton;

        private void Awake()
        {
            loop = new CombatLoop(knobs);
            loop.Reset();
            BuildHud();
        }

        private void Update()
        {
            HandleKeyboard();
            loop.Tick(Time.deltaTime);
            RefreshHud();
        }

        private void HandleKeyboard()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                loop.CycleTarget();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                loop.PullSelected();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                loop.CastSmite();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                loop.CastHeal();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                loop.ToggleMeditation();
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                loop.Stop();
            }
        }

        private void BuildHud()
        {
            document = gameObject.GetComponent<UIDocument>();
            if (document == null)
            {
                document = gameObject.AddComponent<UIDocument>();
            }

            if (document.panelSettings == null)
            {
                var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.name = "CombatFeelPrototypePanel";
                panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                document.panelSettings = panelSettings;
            }

            var root = document.rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1f;
            root.style.backgroundColor = new Color(0.035f, 0.032f, 0.03f, 1f);
            root.style.color = new Color(0.86f, 0.82f, 0.74f, 1f);
            root.style.paddingLeft = 24f;
            root.style.paddingRight = 24f;
            root.style.paddingTop = 18f;
            root.style.paddingBottom = 18f;

            var title = new Label("Combat Feel Prototype");
            title.style.fontSize = 24f;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 8f;
            root.Add(title);

            stateLabel = new Label();
            stateLabel.style.fontSize = 15f;
            stateLabel.style.marginBottom = 14f;
            root.Add(stateLabel);

            var frames = new VisualElement();
            frames.style.flexDirection = FlexDirection.Row;
            frames.style.marginBottom = 14f;
            root.Add(frames);

            var playerPanel = BuildPanel("Cleric");
            frames.Add(playerPanel);
            playerLabel = new Label();
            playerLabel.style.marginBottom = 8f;
            playerPanel.Add(playerLabel);
            healthFill = AddBar(playerPanel, "Health", new Color(0.55f, 0.08f, 0.08f, 1f));
            manaFill = AddBar(playerPanel, "Mana", new Color(0.16f, 0.28f, 0.62f, 1f));

            var targetPanel = BuildPanel("Target");
            targetPanel.style.marginLeft = 14f;
            frames.Add(targetPanel);
            targetLabel = new Label();
            targetLabel.style.marginBottom = 8f;
            targetPanel.Add(targetLabel);
            targetHealthFill = AddBar(targetPanel, "Health", new Color(0.48f, 0.05f, 0.05f, 1f));

            previewLabel = new Label();
            previewLabel.style.marginBottom = 10f;
            previewLabel.style.whiteSpace = WhiteSpace.Normal;
            root.Add(previewLabel);

            var castPanel = BuildPanel("Cast");
            castPanel.style.marginBottom = 12f;
            root.Add(castPanel);
            castLabel = new Label();
            castLabel.style.marginBottom = 8f;
            castPanel.Add(castLabel);
            castFill = AddBar(castPanel, "Cast", new Color(0.78f, 0.68f, 0.35f, 1f));

            var controls = new VisualElement();
            controls.style.flexDirection = FlexDirection.Row;
            controls.style.flexWrap = Wrap.Wrap;
            controls.style.marginBottom = 12f;
            root.Add(controls);

            pullButton = AddButton(controls, "Pull 1", () => loop.PullSelected());
            smiteButton = AddButton(controls, "Smite Q", () => loop.CastSmite());
            healButton = AddButton(controls, "Heal E", () => loop.CastHeal());
            medButton = AddButton(controls, "Sit/Med R", () => loop.ToggleMeditation());
            stopButton = AddButton(controls, "Stop X", () => loop.Stop());

            metricsLabel = new Label();
            metricsLabel.style.marginBottom = 10f;
            root.Add(metricsLabel);

            logLabel = new Label();
            logLabel.style.flexGrow = 1f;
            logLabel.style.whiteSpace = WhiteSpace.Normal;
            logLabel.style.backgroundColor = new Color(0.015f, 0.014f, 0.012f, 1f);
            logLabel.style.borderTopColor = new Color(0.3f, 0.27f, 0.2f, 1f);
            logLabel.style.borderRightColor = new Color(0.3f, 0.27f, 0.2f, 1f);
            logLabel.style.borderBottomColor = new Color(0.3f, 0.27f, 0.2f, 1f);
            logLabel.style.borderLeftColor = new Color(0.3f, 0.27f, 0.2f, 1f);
            logLabel.style.borderTopWidth = 1f;
            logLabel.style.borderRightWidth = 1f;
            logLabel.style.borderBottomWidth = 1f;
            logLabel.style.borderLeftWidth = 1f;
            logLabel.style.paddingLeft = 12f;
            logLabel.style.paddingRight = 12f;
            logLabel.style.paddingTop = 10f;
            logLabel.style.paddingBottom = 10f;
            root.Add(logLabel);
        }

        private VisualElement BuildPanel(string heading)
        {
            var panel = new VisualElement();
            panel.style.flexGrow = 1f;
            panel.style.paddingLeft = 12f;
            panel.style.paddingRight = 12f;
            panel.style.paddingTop = 10f;
            panel.style.paddingBottom = 10f;
            panel.style.backgroundColor = new Color(0.075f, 0.068f, 0.058f, 1f);
            panel.style.borderTopColor = new Color(0.34f, 0.3f, 0.22f, 1f);
            panel.style.borderRightColor = new Color(0.34f, 0.3f, 0.22f, 1f);
            panel.style.borderBottomColor = new Color(0.34f, 0.3f, 0.22f, 1f);
            panel.style.borderLeftColor = new Color(0.34f, 0.3f, 0.22f, 1f);
            panel.style.borderTopWidth = 1f;
            panel.style.borderRightWidth = 1f;
            panel.style.borderBottomWidth = 1f;
            panel.style.borderLeftWidth = 1f;

            var label = new Label(heading);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 6f;
            panel.Add(label);
            return panel;
        }

        private VisualElement AddBar(VisualElement parent, string label, Color fillColor)
        {
            var row = new VisualElement();
            row.style.marginBottom = 6f;
            parent.Add(row);

            var text = new Label(label);
            text.style.fontSize = 12f;
            row.Add(text);

            var background = new VisualElement();
            background.style.height = 14f;
            background.style.backgroundColor = new Color(0.02f, 0.02f, 0.018f, 1f);
            background.style.borderTopColor = new Color(0.18f, 0.16f, 0.13f, 1f);
            background.style.borderRightColor = new Color(0.18f, 0.16f, 0.13f, 1f);
            background.style.borderBottomColor = new Color(0.18f, 0.16f, 0.13f, 1f);
            background.style.borderLeftColor = new Color(0.18f, 0.16f, 0.13f, 1f);
            background.style.borderTopWidth = 1f;
            background.style.borderRightWidth = 1f;
            background.style.borderBottomWidth = 1f;
            background.style.borderLeftWidth = 1f;
            row.Add(background);

            var fill = new VisualElement();
            fill.style.height = 12f;
            fill.style.width = Length.Percent(100f);
            fill.style.backgroundColor = fillColor;
            background.Add(fill);
            return fill;
        }

        private Button AddButton(VisualElement parent, string text, System.Action action)
        {
            var button = new Button(action) { text = text };
            button.style.marginRight = 8f;
            button.style.marginBottom = 8f;
            button.style.minWidth = 96f;
            parent.Add(button);
            return button;
        }

        private void RefreshHud()
        {
            var cleric = loop.Cleric;
            var target = loop.CurrentTarget;
            var preview = loop.PreviewTarget;

            stateLabel.text = $"State: {loop.State} | Pulls: {loop.PullsCompleted}/{loop.PullsGoal} | Tab cycles target, 1 pulls, Q smites, E heals, R sits.";
            playerLabel.text = $"HP {cleric.Health}/{cleric.MaxHealth} | Mana {cleric.Mana}/{cleric.MaxMana} | {(cleric.IsSitting ? "Sitting" : "Standing")}";
            targetLabel.text = target == null ? "No active hostile." : $"{target.Name} | HP {target.Health}/{target.MaxHealth}";
            previewLabel.text = preview == null ? "No remaining preview target." : $"Next pull preview: {preview.Name}. {preview.HauntCue}";
            castLabel.text = loop.CastingSpell == SpellKind.None ? "No active cast." : $"{loop.CastingSpell} casting...";
            metricsLabel.text = loop.BuildMetricsSummary();
            logLabel.text = loop.BuildLogText();

            SetBar(healthFill, cleric.HealthPercent);
            SetBar(manaFill, cleric.ManaPercent);
            SetBar(targetHealthFill, target == null ? 0f : target.HealthPercent);
            SetBar(castFill, loop.CastProgress);

            pullButton.SetEnabled(loop.State == CombatPrototypeState.BetweenPulls);
            smiteButton.SetEnabled(loop.State == CombatPrototypeState.Fighting && target != null);
            healButton.SetEnabled(loop.State == CombatPrototypeState.Fighting);
            medButton.SetEnabled(loop.State == CombatPrototypeState.BetweenPulls);
            stopButton.SetEnabled(loop.State != CombatPrototypeState.Stopped);
        }

        private static void SetBar(VisualElement fill, float percent)
        {
            fill.style.width = Length.Percent(Mathf.Clamp01(percent) * 100f);
        }
    }
}

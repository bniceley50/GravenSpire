#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravenspire.UnityRuntime.Combat
{
    public sealed class CombatJuice : MonoBehaviour
    {
        private static CombatJuice? _instance;
        public static CombatJuice Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = GameObject.Find("CombatJuice");
                    if (go == null)
                    {
                        go = new GameObject("CombatJuice");
                    }
                    _instance = go.GetComponent<CombatJuice>();
                    if (_instance == null)
                    {
                        _instance = go.AddComponent<CombatJuice>();
                    }
                }
                return _instance;
            }
        }

        private struct FloatingText
        {
            public string Text;
            public Vector3 WorldPosition;
            public Color TextColor;
            public float Lifetime;
            public float MaxLifetime;
            public Vector3 Offset;
        }

        private struct FlashEffect
        {
            public Renderer TargetRenderer;
            public Color OriginalBaseColor;
            public Color OriginalColor;
            public float Lifetime;
            public float MaxLifetime;
        }

        private readonly List<FloatingText> _activeTexts = new();
        private readonly List<FlashEffect> _activeFlashes = new();

        private Camera? _mainCamera;
        private GUIStyle? _fctStyle;
        private Vector3 _cameraShakeOffset = Vector3.zero;
        private float _shakeIntensity = 0f;
        private float _shakeDuration = 0f;
        private float _shakeDecay = 1f;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            // Update Camera Shake
            if (_shakeDuration > 0)
            {
                _shakeDuration -= Time.deltaTime;
                _cameraShakeOffset = UnityEngine.Random.insideUnitSphere * _shakeIntensity;
                _shakeIntensity = Mathf.Lerp(_shakeIntensity, 0f, Time.deltaTime * _shakeDecay);
                if (_shakeDuration <= 0)
                {
                    _cameraShakeOffset = Vector3.zero;
                    _shakeIntensity = 0f;
                }
            }

            // Update Material Flashes
            for (int i = _activeFlashes.Count - 1; i >= 0; i--)
            {
                var flash = _activeFlashes[i];
                if (flash.TargetRenderer == null)
                {
                    _activeFlashes.RemoveAt(i);
                    continue;
                }

                flash.Lifetime -= Time.deltaTime;
                if (flash.Lifetime <= 0)
                {
                    // Restore original colors
                    var block = new MaterialPropertyBlock();
                    flash.TargetRenderer.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", flash.OriginalBaseColor);
                    block.SetColor("_Color", flash.OriginalColor);
                    flash.TargetRenderer.SetPropertyBlock(block);

                    _activeFlashes.RemoveAt(i);
                }
                else
                {
                    // Keep active flash
                    _activeFlashes[i] = flash;
                }
            }

            // Update Floating Texts
            for (int i = _activeTexts.Count - 1; i >= 0; i--)
            {
                var text = _activeTexts[i];
                text.Lifetime -= Time.deltaTime;
                text.Offset += new Vector3(0, 1.5f * Time.deltaTime, 0); // Drift up
                if (text.Lifetime <= 0)
                {
                    _activeTexts.RemoveAt(i);
                }
                else
                {
                    _activeTexts[i] = text;
                }
            }
        }

        private void LateUpdate()
        {
            // Apply Camera Shake Offset
            if (_mainCamera != null && _cameraShakeOffset != Vector3.zero)
            {
                _mainCamera.transform.position += _cameraShakeOffset;
            }
        }

        private void OnGUI()
        {
            if (_mainCamera == null) return;

            EnsureStyle();

            var screenHeight = Screen.height;
            foreach (var fct in _activeTexts)
            {
                Vector3 screenPos = _mainCamera.WorldToScreenPoint(fct.WorldPosition);
                if (screenPos.z < 0) continue; // Behind camera

                // Convert Unity screen space to GUI coordinates
                float x = screenPos.x + fct.Offset.x * 100f;
                float y = screenHeight - screenPos.y - fct.Offset.y * 100f;

                float progress = fct.Lifetime / fct.MaxLifetime;
                _fctStyle!.normal.textColor = new Color(fct.TextColor.r, fct.TextColor.g, fct.TextColor.b, progress);
                
                // Shadow / Outline
                var shadowStyle = new GUIStyle(_fctStyle);
                shadowStyle.normal.textColor = new Color(0, 0, 0, progress * 0.75f);
                GUI.Label(new Rect(x - 22, y - 22, 100, 40), fct.Text, shadowStyle);
                GUI.Label(new Rect(x - 20, y - 20, 100, 40), fct.Text, _fctStyle);
            }
        }

        private void EnsureStyle()
        {
            if (_fctStyle == null)
            {
                _fctStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }

        public void SpawnDamageText(string text, Vector3 worldPosition, Color color)
        {
            _activeTexts.Add(new FloatingText
            {
                Text = text,
                WorldPosition = worldPosition + UnityEngine.Random.insideUnitSphere * 0.15f,
                TextColor = color,
                Lifetime = 1.0f,
                MaxLifetime = 1.0f,
                Offset = Vector3.zero
            });
        }

        public void TriggerCameraShake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeDuration = duration;
        }

        public void Flash(Renderer renderer, Color flashColor, float duration = 0.15f)
        {
            if (renderer == null || renderer.sharedMaterial == null) return;

            // Retrieve current colors
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            
            Color originalBase = renderer.sharedMaterial.HasProperty("_BaseColor") 
                ? renderer.sharedMaterial.GetColor("_BaseColor") 
                : Color.white;
            Color originalCol = renderer.sharedMaterial.HasProperty("_Color") 
                ? renderer.sharedMaterial.GetColor("_Color") 
                : Color.white;

            // Check if already flashing to avoid losing original color
            bool alreadyFlashing = false;
            for (int i = 0; i < _activeFlashes.Count; i++)
            {
                if (_activeFlashes[i].TargetRenderer == renderer)
                {
                    var existing = _activeFlashes[i];
                    existing.Lifetime = duration; // extend flash duration
                    _activeFlashes[i] = existing;
                    alreadyFlashing = true;
                    break;
                }
            }

            if (!alreadyFlashing)
            {
                _activeFlashes.Add(new FlashEffect
                {
                    TargetRenderer = renderer,
                    OriginalBaseColor = originalBase,
                    OriginalColor = originalCol,
                    Lifetime = duration,
                    MaxLifetime = duration
                });
            }

            // Apply flash color
            block.SetColor("_BaseColor", flashColor);
            block.SetColor("_Color", flashColor);
            renderer.SetPropertyBlock(block);
        }
    }
}

using FPSGame.Rounds;
using UnityEngine;

namespace FPSGame.Combat
{
    [DisallowMultipleComponent]
    public class WorldHealthBar : MonoBehaviour
    {
        private static GUIStyle labelStyle;
        private static Texture2D whiteTexture;

        [SerializeField] private Health health;
        [SerializeField] private TeamMember teamMember;
        [SerializeField] private Vector3 worldOffset = new(0f, 2.25f, 0f);
        [SerializeField] private Vector2 barSize = new(64f, 8f);
        [SerializeField] private bool showNumericHealth = true;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (teamMember == null)
            {
                teamMember = GetComponent<TeamMember>();
            }
        }

        private void OnGUI()
        {
            EnsureGuiResources();

            if (health == null || Camera.main == null || !health.IsAlive)
            {
                return;
            }

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position + worldOffset);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            float health01 = Mathf.Clamp01(health.CurrentHealth / 100f);
            float screenX = screenPoint.x - (barSize.x * 0.5f);
            float screenY = Screen.height - screenPoint.y;
            Rect backRect = new(screenX, screenY, barSize.x, barSize.y);
            Rect fillRect = new(screenX + 1f, screenY + 1f, (barSize.x - 2f) * health01, barSize.y - 2f);

            DrawRect(backRect, new Color(0f, 0f, 0f, 0.65f));
            DrawRect(fillRect, GetTeamColor());

            if (showNumericHealth)
            {
                GUI.Label(new Rect(screenX - 6f, screenY - 18f, barSize.x + 12f, 18f), Mathf.CeilToInt(health.CurrentHealth).ToString(), labelStyle);
            }
        }

        private Color GetTeamColor()
        {
            if (teamMember == null)
            {
                return new Color(0.8f, 0.8f, 0.8f, 1f);
            }

            return teamMember.Side == TeamSide.Terrorists
                ? new Color(1f, 0.35f, 0.15f, 1f)
                : new Color(0.15f, 0.7f, 1f, 1f);
        }

        private static void EnsureGuiResources()
        {
            if (whiteTexture == null)
            {
                whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                whiteTexture.SetPixel(0, 0, Color.white);
                whiteTexture.Apply();
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11
                };
                labelStyle.normal.textColor = Color.white;
            }
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }
    }
}

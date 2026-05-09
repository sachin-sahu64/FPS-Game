using UnityEngine;

namespace FPSGame.Objectives
{
    public class BombSite : MonoBehaviour
    {
        [SerializeField] private string siteId = "A";
        [SerializeField] private float plantRadius = 2.5f;

        public string SiteId => siteId;

        public float PlantRadius => plantRadius;
    }
}

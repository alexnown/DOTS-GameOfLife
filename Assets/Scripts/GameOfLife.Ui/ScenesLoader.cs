using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameOfLife
{
    public class ScenesLoader : MonoBehaviour
    {
        [SerializeField]
        private GameObject _scenesListContainer = null;
        [SerializeField]
        private GameObject _closeBtnContainer = null;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            _closeBtnContainer.SetActive(false);
        }

        public void ClearWorldAndShowScenesList()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            em.DestroyEntity(em.UniversalQuery);
            _scenesListContainer.SetActive(true);
            _closeBtnContainer.SetActive(false);
        }

        public void LoadScene(int i)
        {
            SceneManager.LoadScene(i);
            _scenesListContainer.SetActive(false);
            _closeBtnContainer.SetActive(true);
        }
    }
}

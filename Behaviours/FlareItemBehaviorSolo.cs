using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

/* NOTES FOR THE DEV
 * 
 * Netcode for Multiplayer
 * Animations for use and throw
 * Lag when player stands in smoke
 * 
 */

namespace FlareItemMod.Behaviours
{
    internal class FlareItemBehaviorSolo : PhysicsProp
    {
        public GameObject newModel;
        public GameObject burningModel;
        public Light fireSource;
        public VisualEffect smokeEffect;
        public enum FlareState { NEW, BURNING, DEAD }
        private FlareState currentFlareState = FlareState.NEW; 
        public AudioSource audioSource;
        public AudioClip initialBurningClip;
        public AudioClip loopBurningClip;
        public AudioClip deadClip;
        private float timer = 0.0f;
        public float timeToBurn = 300.0f;

        public void Awake()
        {
            fireSource = gameObject.GetComponentInChildren<Light>();
            if (fireSource == null)
            {
                Plugin.mls.LogError("FlareItemBehavior: Missing Light component.");
            }
            else
            {
                fireSource.enabled = false;
            }

            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Plugin.mls.LogError("FlareItemBehavior: Missing AudioSource component.");
            }
            else
            {
                audioSource.playOnAwake = false;
                audioSource.loop = false;
            }

            smokeEffect = gameObject.GetComponentInChildren<VisualEffect>();
            if (smokeEffect == null)
            {
                Plugin.mls.LogError("FlareItemBehavior: Missing ParticleSystem component.");
            }
            else
            {
               smokeEffect.Stop();
            }

            newModel = transform.Find("CappedFlare_FIXED").gameObject;
            if (newModel == null)
            {
                Plugin.mls.LogError("FlareItemBehavior: Missing New Model GameObject.");
            }
            else
            {
                newModel.transform.localScale = Vector3.one;
            }

            burningModel = transform.Find("UncappedFlare_FIXED").gameObject;
            if (burningModel == null)
            {
                Plugin.mls.LogError("FlareItemBehavior: Missing Burning Model GameObject.");
            }
            else
            {
                burningModel.transform.localScale = Vector3.zero;
            }

            Plugin.mls.LogInfo("Item initialized in state: " + currentFlareState);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown)
            {
                switch (currentFlareState)
                {
                    case FlareState.NEW:
                        ChangeState(FlareState.BURNING);
                        break;
                    case FlareState.BURNING:
                    case FlareState.DEAD:
                        Throw();
                        break;
                    default:
                        Plugin.mls.LogWarning("This isn't supposed to happen... Flare State Invalid");
                        break;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            switch (currentFlareState)
            {
                //  If the state is burning and full length of flame has been reached
                //     state -> DEAD
                case FlareState.BURNING:
                    timer += Time.deltaTime;
                    if (timer >= timeToBurn)
                    {
                        ChangeState(FlareState.DEAD);
                    }
                    break;
            }
        }

        public override void EquipItem()
        {
            base.EquipItem();
            if(currentFlareState == FlareState.BURNING)
            {
                fireSource.intensity = 1000;
                smokeEffect.Play();
                PlayLoopingBurningClip();
            }
        }

        public override void PocketItem()
        {
            base.PocketItem();
            if(currentFlareState == FlareState.BURNING)
            {
                audioSource.Stop();
                fireSource.intensity = 0;
                smokeEffect.Stop();
            }
        }

        public Vector3 GetFlareThrowDestination()
        {
            Vector3 position = base.transform.position;
            Ray flareThrowRay = new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward);
            RaycastHit flareHit;
            position = ((!Physics.Raycast(flareThrowRay, out flareHit, 12f, StartOfRound.Instance.collidersAndRoomMaskAndDefault)) ? flareThrowRay.GetPoint(10f) : flareThrowRay.GetPoint(flareHit.distance - 0.05f));
            flareThrowRay = new Ray(position, Vector3.down);
            if (Physics.Raycast(flareThrowRay, out flareHit, 30f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                return flareHit.point + Vector3.up * 0.05f;
            }
            return flareThrowRay.GetPoint(30f);
        }

        private void Throw()
        {
            playerHeldBy.DiscardHeldObject(placeObject: true, null, GetFlareThrowDestination());
        }

        private void PlayLoopingBurningClip()
        {
            audioSource.loop = true;
            audioSource.clip = loopBurningClip;
            audioSource.Play();
        }
        private IEnumerator FadeOutLight()
        {
            float fadeDuration = 1.0f; 
            float startIntensity = fireSource.intensity;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                fireSource.intensity = Mathf.Lerp(startIntensity, 0, t / fadeDuration);
                yield return null;
            }
            fireSource.enabled = false; 
        }

        private IEnumerator FadeInLight()
        {
            float startIntensity = 0;
            float fadeDuration = 5.0f; 
            float targetIntensity = 1000; 
            fireSource.enabled = true; 
            fireSource.intensity = startIntensity;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                fireSource.intensity = Mathf.Lerp(startIntensity, targetIntensity, t / fadeDuration);
                yield return null; 
            }
            fireSource.intensity = targetIntensity;
        }


        private void ChangeState(FlareState newState)
        {
            Plugin.mls.LogInfo("Changing state from " + currentFlareState + " to " + newState);
            currentFlareState = newState;
            switch (newState)
            {
                case FlareState.BURNING:
                    burningModel.transform.localScale = Vector3.one;
                    newModel.transform.localScale = Vector3.zero;
                    smokeEffect.Play();
                    StartCoroutine(FadeInLight());
                    audioSource.PlayOneShot(initialBurningClip);
                    Invoke(nameof(PlayLoopingBurningClip), initialBurningClip.length);
                    timer = 0.0f;
                    break;
                case FlareState.DEAD:
                    smokeEffect.Stop();
                    StartCoroutine(FadeOutLight());
                    audioSource.Stop();
                    audioSource.PlayOneShot(deadClip);
                    break;
                default:
                    break;
            }
        }
    }
}
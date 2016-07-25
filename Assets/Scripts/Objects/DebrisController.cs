﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebrisController : SpaceObject {

	// Configuration parameters.
	public float minRandomScale;
	public float maxRandomScale;
	public float scaleAxisVariability; // Allow non-uniform scale along axis.
	public float velocityVariablity;
	public float maxRandomTumble;

	public List<GameObject> models;
	public GameObject dieExplosionPrefab;
	public float explosionTime = 1f;

	// The player scores points when he/she destroys the debris.
	public int scoreValue = 1;

	private GameObject activeModel;

	/*
	 * Compute a random scale, random orientation, a mass, life points and a random velocity.
	 * Should be called at the instantiation of the debris.
	 */
	public void ConfigurateDebris (Vector3 velocity) {
		Rigidbody rigidbody = GetComponent<Rigidbody> ();

		// Choose a random model
		int modelIndex = Random.Range(0, models.Count);
		GetComponent<MeshFilter> ().mesh = models[modelIndex].GetComponent<MeshFilter> ().sharedMesh;

		// Random scale
		Transform parent = transform.parent;
		transform.parent = null;

		Vector3 randomScale = Vector3.one * Random.value * (maxRandomScale - minRandomScale) + Vector3.one * minRandomScale // Random axis-uniform scale
							+ (Random.insideUnitSphere * scaleAxisVariability);
		transform.localScale = randomScale;

		transform.parent = parent;

		// Random orientation
		transform.rotation = Random.rotation;

		// Random life points
		lifeShieldManager.LifePoints *= Mathf.Max(Mathf.RoundToInt(transform.localScale.magnitude), lifeShieldManager.LifePoints);

		// Random mass
		rigidbody.mass *= transform.localScale.magnitude;

		// Random velocity
		rigidbody.AddForce (velocity + Random.insideUnitSphere * velocityVariablity, ForceMode.VelocityChange);
		rigidbody.AddTorque (Random.insideUnitSphere * maxRandomTumble, ForceMode.VelocityChange);
	}

	/*
	 * Update score and destroy when life points drops to zero.
	 */
	protected override void OnLifeShieldUpdated () {
		if (lifeShieldManager.LifePoints <= LifeShieldManager.MIN_LIFE_POINTS) {
			scoreManager.Score += scoreValue;
			StartCoroutine ("Die");
		}
	}

	/*
	 * Instantiate the explosion, hide the model and destroy the object after a delay.
	 */
	IEnumerator Die () {
		GameObject explosion = (GameObject)Instantiate (dieExplosionPrefab, transform.position, transform.rotation);
		explosion.transform.parent = this.transform;
		explosion.transform.localScale = Vector3.one;

		GetComponent<SphereCollider> ().enabled = false;
		GetComponent<MeshRenderer> ().enabled = false;

		yield return new WaitForSeconds(explosionTime);
		Destroy (gameObject);

		yield return null;
	}
}

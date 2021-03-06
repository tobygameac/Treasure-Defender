﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(SphereCollider))]
public class LaserCannon : MonoBehaviour {

  public AudioClip laserSound;

  public GameObject laser;
  public float turningSpeed;

  private float reloadTime;
  private float attackingSpeed;
  private float attackingRange;

  public Transform[] muzzles;
  public Transform turretBall;

  private float nextFireTime;

  private Transform target;

  private CharacterStats characterStats;

  private static Game game;

  void Start() {
    characterStats = GetComponent<CharacterStats>();

    attackingSpeed = characterStats.AttackingSpeed;

    if (attackingSpeed <= 1e-8f) {
      attackingSpeed = 1e-8f;
    }

    reloadTime = (1 / attackingSpeed);

    GetComponent<SphereCollider>().radius = characterStats.AttackingRange;

    target = null;

    nextFireTime = Time.time;

    if (game == null) {
      game = Camera.main.GetComponent<Game>();
    }
  }

  void Update() {
    if (target != null) {
      Quaternion desiredRotation = Quaternion.LookRotation(transform.InverseTransformDirection(target.position - turretBall.position));

      desiredRotation.eulerAngles = new Vector3(turretBall.localEulerAngles.x, desiredRotation.eulerAngles.y, turretBall.localEulerAngles.z); // y-axis only

      turretBall.localRotation = Quaternion.Slerp(turretBall.localRotation, desiredRotation, Time.deltaTime * turningSpeed);

      float angleToEnemy = Quaternion.Angle(turretBall.localRotation, desiredRotation);

      if (Time.time >= nextFireTime && angleToEnemy <= 5) {
        FireProjectile();
      }
    }
  }

  void OnTriggerEnter(Collider collider) {
    if (target == null && collider.gameObject.tag == "Enemy") {
      target = collider.gameObject.transform;
    }
  }

  void OnTriggerStay(Collider collider) {
    if (target == null && collider.gameObject.tag == "Enemy") {
      target = collider.gameObject.transform;
    }
  }

  void OnTriggerExit(Collider collider) {
    if (target == collider.transform) {
      target = null;
    }
  }

  void FireProjectile() {
    AudioManager.PlayAudioClip(laserSound);

    nextFireTime = Time.time + reloadTime;

    for (int i = 0; i < muzzles.Length; ++i) {
      GameObject laserGameObject = Instantiate(laser, muzzles[i].position, Quaternion.identity) as GameObject;

      laserGameObject.transform.parent = game.gameSceneParentTransform;

      laserGameObject.GetComponent<Laser>().TargetPosition = target.position;
      CharacterStats targetCharacterStats = target.GetComponent<CharacterStats>();
      targetCharacterStats.CurrentHP -= characterStats.Damage / muzzles.Length;
      if (targetCharacterStats.CurrentHP <= 0) {
        ++characterStats.UnitKilled;
        if (game.HasTechnology(GameConstants.TechnologyID.SELF_LEARNING)) {
          characterStats.DamageModifier += GameConstants.SELF_LEARNING_IMPROVEMENT_PERCENT_PER_KILL;
        } else {
        }
      }
    }
  }
}

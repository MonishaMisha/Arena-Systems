using PortfolioShowcase.Gameplay.Weapons;
using UnityEngine;

namespace PortfolioShowcase.Gameplay.Player
{
    /// <summary>
    /// Uses the classic Input Manager (Input.GetAxis) rather than the Input
    /// System package to keep the project dependency-free. See README for the
    /// one Project Settings tweak this requires on a fresh Unity 6 project.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private WeaponController weapon;

        private CharacterController _controller;
        private Camera _camera;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _camera = Camera.main;
        }

        private void Update()
        {
            HandleMovement();
            HandleAim();
            HandleShooting();
        }

        private void HandleMovement()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f) input.Normalize();

            _controller.SimpleMove(input * moveSpeed);
        }

        // Twin-stick style: face the mouse cursor projected onto the ground plane,
        // so movement and aiming are independent.
        private void HandleAim()
        {
            if (_camera == null) return;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var ground = new Plane(Vector3.up, transform.position);
            if (!ground.Raycast(ray, out var distance)) return;

            var look = ray.GetPoint(distance) - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f) transform.forward = look;
        }

        private void HandleShooting()
        {
            if (Input.GetButton("Fire1")) weapon.TryFire();
        }
    }
}

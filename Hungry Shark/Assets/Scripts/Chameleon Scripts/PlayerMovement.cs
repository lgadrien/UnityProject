using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float walkSpeed = 8f;
    public float runSpeed = 12f;
    public float rotationSpeed = 720f; // Vitesse de rotation en degrés par seconde
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public Camera playerCamera; // Référence à la caméra du joueur

    private Vector3 velocity;
    private bool isGrounded;
    private bool isRunning;
    private Animator animator; // Référence au composant Animator

    void Start()
    {
        animator = GetComponent<Animator>(); // Obtenir la référence au composant Animator
        Cursor.lockState = CursorLockMode.Locked; // Verrouiller le curseur au centre de l'écran

        // Assigner la caméra principale si elle n'est pas déjà assignée
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Vérification pour s'assurer que la caméra est assignée
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera not assigned in the Inspector and no Main Camera found in the scene.");
        }

        // Vérification pour s'assurer que l'Animator est assigné
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
    }

    void Update()
    {
        // Si la caméra n'est pas assignée, sortir de la méthode Update
        if (playerCamera == null || animator == null)
        {
            return;
        }

        // Vérifier si le personnage est au sol
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Applique une petite force vers le bas pour coller au sol
        }

        // Obtenir l'entrée du joueur
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Vérifier si le personnage doit courir
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // Vérifier si le personnage doit se déplacer
        if (direction.magnitude >= 0.1f)
        {
            // Calculer la direction de la caméra
            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 cameraRight = playerCamera.transform.right;

            // Ignorer la composante verticale
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            // Normaliser les directions
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculer le vecteur de mouvement en fonction de la direction de la caméra
            Vector3 moveDir = (cameraForward * vertical + cameraRight * horizontal).normalized;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            controller.Move(moveDir * currentSpeed * Time.deltaTime);

            // Activer l'animation de marche/course
            animator.SetBool("IsWalking", true);
            animator.SetFloat("Speed", isRunning ? currentSpeed * 2f : currentSpeed);

            // Faire tourner le personnage vers la direction du mouvement de manière fluide
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Garder l'animation de marche/course activée même si le personnage ne se déplace pas
            animator.SetBool("IsWalking", true);
            // Désactiver l'animation de course pour éviter que le personnage ne coure sans bouger
            animator.SetFloat("Speed", walkSpeed);

            // Réinitialiser la vitesse du personnage à zéro pour l'empêcher de dériver
            controller.Move(Vector3.zero);
        }

        // Appliquer la gravité
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (Input.GetButtonDown("Jump"))
        {
            // Gérer le saut
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Appliquer le mouvement vertical (gravité et saut)
        controller.Move(velocity * Time.deltaTime);
    }
}

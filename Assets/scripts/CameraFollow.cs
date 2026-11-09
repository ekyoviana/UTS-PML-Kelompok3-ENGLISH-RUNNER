using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Tarik objek Player ke sini di Inspector
    public Vector3 offset;    // Jarak aman kamera dari player (misal: X=0, Y=3, Z=-5)
    public float smoothSpeed = 0.125f; // Kecepatan kamera mengikuti player

    void LateUpdate()
    {
        if (player != null)
        {
            // Tentukan posisi tujuan kamera
            Vector3 desiredPosition = player.position + offset;
            
            // Khusus Endless Runner: Jika Anda TIDAK ingin kamera ikut naik-turun saat player melompat,
            // Anda bisa mengunci sumbu Y dengan menghapus komentar di bawah ini:
            // desiredPosition.y = transform.position.y;

            // Haluskan pergerakan kamera menuju posisi tujuan
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            
            // Ubah posisi kamera
            transform.position = smoothedPosition;
        }
    }
}

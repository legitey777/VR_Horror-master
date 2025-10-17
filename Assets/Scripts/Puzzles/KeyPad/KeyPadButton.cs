using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace ldw
{
    public class KeyPadButton : PushButtonInteractor
    {
        // Ű �е� ��ȣ
        public int keyPadNum;

        // Ű �е� Ŭ�� �� �߻��Ǵ� �̺�Ʈ
        [SerializeField]
        UnityEvent KeyPadClicked;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("KeyPad"))
                OnKeyPadButtonClicked();
        }

        public void OnKeyPadButtonClicked()
        {
            Debug.Log($"insert {keyPadNum}");

            KeyPadClicked?.Invoke();
        }
    }
}
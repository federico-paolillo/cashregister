import { type ReactNode, useEffect, useId, useRef } from "react";
import { ModalIdProvider } from "@cashregister/components/use-modal";

interface ModalProps {
  open: boolean;
  onClose: () => void;
  children: ReactNode;
}

export function Modal({ open, onClose, children }: ModalProps) {
  const id = useId();
  const dialogRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = dialogRef.current;

    if (dialog) {
      if (open) {
        dialog.showModal();
      } else {
        dialog.close();
      }
    }
  }, [open]);

  return (
    <ModalIdProvider id={id}>
      <dialog
        id={id}
        ref={dialogRef}
        closedby="none"
        className="fixed inset-0 m-auto max-h-[90vh] max-w-[90vw] w-full h-full overflow-hidden backdrop:bg-black/50"
        onClose={onClose}
      >
        {children}
      </dialog>
    </ModalIdProvider>
  );
}

import {
  createContext,
  type ReactNode,
  useCallback,
  useContext,
  useEffect,
  useId,
  useRef,
  useState,
} from "react";

const ModalIdContext = createContext<string | null>(null);

export function useModalId(): string | null {
  return useContext(ModalIdContext);
}

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
    if (!dialog) return;

    if (open && !dialog.open) {
      dialog.showModal();
    } else if (!open && dialog.open) {
      dialog.close();
    }
  }, [open]);

  return (
    <ModalIdContext.Provider value={id}>
      <dialog
        id={id}
        ref={dialogRef}
        closedby="none"
        className="overflow-hidden"
        onClose={onClose}
      >
        {children}
      </dialog>
    </ModalIdContext.Provider>
  );
}

export function useModal() {
  const [isOpen, setIsOpen] = useState(false);
  const open = useCallback(() => setIsOpen(true), []);
  const close = useCallback(() => setIsOpen(false), []);
  return { isOpen, open, close };
}

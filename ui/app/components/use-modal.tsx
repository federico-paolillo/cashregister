import {
  createContext,
  type ReactNode,
  useCallback,
  useContext,
  useState,
} from "react";

const ModalIdContext = createContext<string | undefined>(undefined);

export function ModalIdProvider({
  id,
  children,
}: {
  id: string;
  children: ReactNode;
}) {
  return (
    <ModalIdContext.Provider value={id}>{children}</ModalIdContext.Provider>
  );
}

export function useModalId(): string {
  const id = useContext(ModalIdContext);
  if (id === undefined) {
    throw new Error("useModalId must be used within a <Modal>");
  }
  return id;
}

export function useModal() {
  const [isOpen, setIsOpen] = useState(false);
  const open = useCallback(() => setIsOpen(true), []);
  const close = useCallback(() => setIsOpen(false), []);
  return { isOpen, open, close };
}

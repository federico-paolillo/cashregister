import {
  createContext,
  type ReactNode,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";

export interface ErrorMessage {
  id: number;
  message: string;
}

interface ErrorMessagesContextValue {
  errors: ErrorMessage[];
  addError: (message: string) => void;
  dismissError: (id: number) => void;
}

const ErrorMessagesContext = createContext<
  ErrorMessagesContextValue | undefined
>(undefined);

export function useErrorMessages(): ErrorMessagesContextValue {
  const ctx = useContext(ErrorMessagesContext);

  if (ctx === undefined) {
    throw new Error(
      "useErrorMessages must be used within an <ErrorMessagesProvider>",
    );
  }

  return ctx;
}

interface ErrorMessagesProviderProps {
  autoDismissMs?: number;
  children: ReactNode;
}

export function ErrorMessagesProvider({
  autoDismissMs = 5000,
  children,
}: ErrorMessagesProviderProps) {
  const { errors, addError, dismissError } =
    useErrorMessagesState(autoDismissMs);

  return (
    <ErrorMessagesContext.Provider value={{ errors, addError, dismissError }}>
      {children}
    </ErrorMessagesContext.Provider>
  );
}

export function useErrorMessagesState(autoDismissMs: number) {
  const [errors, setErrors] = useState<ErrorMessage[]>([]);
  const nextId = useRef(0);
  const timers = useRef(new Map<number, ReturnType<typeof setTimeout>>());

  const dismissError = useCallback((id: number) => {
    setErrors((prev) => prev.filter((e) => e.id !== id));

    const timer = timers.current.get(id);
    if (timer !== undefined) {
      clearTimeout(timer);
      timers.current.delete(id);
    }
  }, []);

  const addError = useCallback(
    (message: string) => {
      const id = nextId.current++;

      setErrors((prev) => [...prev, { id, message }]);

      if (autoDismissMs > 0) {
        const timer = setTimeout(() => {
          timers.current.delete(id);
          dismissError(id);
        }, autoDismissMs);

        timers.current.set(id, timer);
      }

      return id;
    },
    [autoDismissMs, dismissError],
  );

  useEffect(() => {
    const currentTimers = timers.current;

    return () => {
      for (const timer of currentTimers.values()) {
        clearTimeout(timer);
      }
      currentTimers.clear();
    };
  }, []);

  return { errors, addError, dismissError };
}

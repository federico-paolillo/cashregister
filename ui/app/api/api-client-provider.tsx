import { createContext, useContext } from "react";
import type { ReactNode } from "react";
import type { ApiClient } from "./api-client";

const ApiClientContext = createContext<ApiClient | undefined>(undefined);

export function ApiClientProvider({
  client,
  children,
}: {
  client: ApiClient;
  children: ReactNode;
}) {
  return (
    <ApiClientContext.Provider value={client}>
      {children}
    </ApiClientContext.Provider>
  );
}

export function useApiClient(): ApiClient {
  const client = useContext(ApiClientContext);
  if (client === undefined) {
    throw new Error("useApiClient must be used within an ApiClientProvider");
  }
  return client;
}

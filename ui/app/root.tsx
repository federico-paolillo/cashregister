import { StrictMode } from "react";
import { Links, Meta, Outlet, Scripts, ScrollRestoration, useRouteError, isRouteErrorResponse } from "react-router";
import "@cashregister/app.css";
import { ErrorMessagesProvider } from "@cashregister/components/use-error-messages";
import { ErrorMessageList } from "@cashregister/components/error-message-list";
import { Spinner } from "@cashregister/components/spinner";

export function Layout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <head>
        <meta charSet="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <Meta />
        <Links />
      </head>
      <body>
        <StrictMode>
          <ErrorMessagesProvider>
            <ErrorMessageList />
            <div className="flex h-screen flex-col">
              {children}
            </div>
          </ErrorMessagesProvider>
        </StrictMode>
        <ScrollRestoration />
        <Scripts />
      </body>
    </html>
  );
}

export default function Root() {
  return <Outlet />;
}

export function HydrateFallback() {
  return <Spinner />;
}

export function ErrorBoundary() {
  const error = useRouteError();

  let message = "An unexpected error occurred.";
  if (isRouteErrorResponse(error)) {
    message = error.data || `${error.status} ${error.statusText}`;
  } else if (error instanceof Error) {
    message = error.message;
  }

  return (
    <div className="flex flex-1 items-center justify-center">
      <div className="text-center">
        <h1 className="text-xl font-semibold text-red-600">Something went wrong</h1>
        <p className="mt-2 text-gray-600">{message}</p>
        <button
          type="button"
          onClick={() => window.location.reload()}
          className="mt-4 btn-primary"
        >
          Try Again
        </button>
      </div>
    </div>
  );
}

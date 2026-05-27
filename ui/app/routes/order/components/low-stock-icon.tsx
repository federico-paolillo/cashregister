export function LowStockIcon() {
  return (
    <span
      role="img"
      aria-label="Low stock"
      className="inline-flex h-5 w-5 items-center justify-center rounded border border-amber-300 bg-amber-50 text-amber-700"
    >
      <svg
        aria-hidden="true"
        viewBox="0 0 24 24"
        className="h-3.5 w-3.5"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="square"
        strokeLinejoin="miter"
      >
        <path d="M12 3 2 21h20L12 3Z" />
        <path d="M12 9v5" />
        <path d="M12 17h.01" />
      </svg>
    </span>
  );
}

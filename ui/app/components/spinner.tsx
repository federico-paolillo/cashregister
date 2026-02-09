export function Spinner() {
  return (
    <div className="absolute inset-0 z-10 flex items-center justify-center bg-gray-200/50">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-500 border-t-transparent"></div>
    </div>
  );
}

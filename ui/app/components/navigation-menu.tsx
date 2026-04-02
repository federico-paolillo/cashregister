import { useEffect, useRef, useState } from "react";
import { Link, NavLink, useLocation } from "react-router";

export default function NavigationMenu() {
  const [menuOpen, setMenuOpen] = useState(false);

  const menuRef = useRef<HTMLDivElement>(null);

  const location = useLocation();

  useEffect(() => {
    setMenuOpen(false);
  }, [location]);

  useEffect(() => {
    if (!menuOpen) {
      return;
    }

    function handleClickOutside(e: MouseEvent) {
      if (menuRef.current) {
        // Did we click INSIDE the menu ? Yes ? Do not close it
        if (menuRef.current.contains(e.target as Node)) {
          return;
        }
      }

      setMenuOpen(false);
    }

    document.addEventListener("mousedown", handleClickOutside);

    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [menuOpen]);

  return (
    <nav className="w-full p-4 bg-blue-600 grid grid-cols-3 items-center">
      <div className="flex items-center gap-4">
        <img alt="Cashregister logo" src="/icons/printer.svg" className="h-8 w-8 text-white" />
        <span className="text-white font-semibold">Cashregister</span>
      </div>
      <ul className="flex flex-row gap-16 justify-center">
        <li><NavLink className={({ isActive }) => isActive ? "active-nav-link" : "inactive-nav-link"} to="/">Place</NavLink></li>
        <li><NavLink className={({ isActive }) => isActive ? "active-nav-link" : "inactive-nav-link"} to="/articles">Articles</NavLink></li>
        <li><NavLink className={({ isActive }) => isActive ? "active-nav-link" : "inactive-nav-link"} to="/orders">Orders</NavLink></li>
      </ul>
      <div ref={menuRef} className="justify-self-end relative">
        <button type="button" onClick={() => setMenuOpen(o => !o)} className="text-white hover:text-gray-200 text-2xl leading-none cursor-pointer" aria-label="Open menu" aria-expanded={menuOpen}>
          &#9776;
        </button>
        {menuOpen && (
          <ul className="absolute right-0 top-full mt-2 rounded shadow-lg py-1 min-w-36 z-10 bg-white">
            <li><Link className="block px-4 py-2 text-black hover:text-gray-700" to="/devices">Devices</Link></li>
          </ul>
        )}
      </div>
    </nav>
  );
}
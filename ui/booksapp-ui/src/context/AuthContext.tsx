import {
  createContext,
  useContext,
  useState,
  useEffect,
  type ReactNode,
} from "react";
import type { User } from "../types/User";
import { AuthAgent } from "../api/authAgent";

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (
    email: string,
    password: string,
    displayName: string
  ) => Promise<void>;
  logout: () => void;
  error: string | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Check for existing token on startup
  useEffect(() => {
    const storedUser = localStorage.getItem("user");
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
  }, []);

  const login = async (email: string, password: string) => {
    setError(null);
    try {
      const data = await AuthAgent.login(email, password);

      const user: User = {
        displayName: data.displayName,
        token: data.token,
        // email & role remain undefined here, keeping them hidden
      };

      setUser(user);
      localStorage.setItem("user", JSON.stringify(user));      
    } catch (err: any) {
      setError(err.message);
      throw err;
    }
  };

  const register = async (
    email: string,
    password: string,
    displayName: string
  ) => {
    setError(null);
    try {
      // We default 'role' to 'User' here or let the agent handle it
      const data = await AuthAgent.register(
        email,
        password,
        displayName,
        "User"
      );
      setUser(data);
      localStorage.setItem("user", JSON.stringify(data));
    } catch (err: any) {
      setError(err.message);
      throw err;
    }
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem("user");
    // Optional: Redirect to home
    window.location.href = "/";
  };

  return (
    <AuthContext.Provider
      value={{ user, isAuthenticated: !!user, login, register, logout, error }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within an AuthProvider");
  return context;
};

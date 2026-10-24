import {Navigate, Route, Routes} from "react-router-dom";
import './App.css'
import LandingPage from "./pages/LandingPage.tsx";
import {
    ChangePasswordPage,
    ConfirmEmailPage,
    ForgotPasswordPage,
    LoginPage,
    RegisterPage,
    ResendConfirmationPage,
    ResetPasswordPage,
} from "./pages/AuthPages.tsx";
import PartnerDashboard from "./pages/PartnerDashboard.tsx";

function App() {


  return (
      <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
          <Route path="/confirm-email" element={<ConfirmEmailPage />} />
          <Route path="/partner/confirm-email" element={<ConfirmEmailPage />} />
          <Route path="/resend-confirmation" element={<ResendConfirmationPage />} />
          <Route path="/change-password" element={<ChangePasswordPage />} />
          <Route path="/dashboard" element={<PartnerDashboard />} />
          <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
  )
}

export default App

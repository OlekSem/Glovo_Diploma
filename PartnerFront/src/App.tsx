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
import PartnerProfilePage from "./pages/PartnerProfilePage.tsx";
import CreateCompanyPage from "./pages/CreateCompanyPage.tsx";
import CompanyRequestsPage from "./pages/CompanyRequestsPage.tsx";
import ActiveCompaniesPage from "./pages/ActiveCompaniesPage.tsx";
import CompanyManagementPage from "./pages/CompanyManagementPage.tsx";

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
          <Route path="/profile" element={<PartnerProfilePage />} />
          <Route path="/companies/new" element={<CreateCompanyPage />} />
          <Route path="/companies" element={<ActiveCompaniesPage />} />
          <Route path="/companies/:companyId" element={<CompanyManagementPage />} />
          <Route path="/company-requests" element={<CompanyRequestsPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
  )
}

export default App

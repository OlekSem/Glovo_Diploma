import {Route, Routes} from "react-router-dom";
import './App.css'
import LandingPage from "./pages/LandingPage.tsx";

function App() {


  return (
      <Routes>
          <Route path="/" element={<LandingPage />} />

          {/* Example of routes for authenticated users */}
          {/*<Route path="/admin" element={<AdminRoute />}>*/}
          {/*    <Route element={<AppLayout />}>*/}
          {/*        <Route index element={<Dashboard />} />*/}
          {/*    </Route>*/}
          {/*</Route>*/}
      </Routes>
  )
}

export default App

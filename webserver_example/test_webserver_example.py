import base64
import importlib
import tempfile
import unittest
from pathlib import Path


try:
    server = importlib.import_module("webserver_example.webserver_example")
    dependencies_available = True
except ImportError:
    server = None
    dependencies_available = False


@unittest.skipUnless(dependencies_available, "install webserver_example/requirements.txt")
class WebServerExampleTests(unittest.TestCase):
    def setUp(self):
        self.temp_dir = tempfile.TemporaryDirectory()
        server.OUTPUT_DIR = Path(self.temp_dir.name).resolve()
        server.users = {"tester": server.generate_password_hash("secret")}
        self.client = server.app.test_client()
        token = base64.b64encode(b"tester:secret").decode("ascii")
        self.auth_header = {"Authorization": "Basic " + token}

    def tearDown(self):
        self.temp_dir.cleanup()

    def test_rejects_paths_outside_output_directory(self):
        response = self.client.post(
            "/form",
            data={"filepath": "../outside.txt", "data": "blocked"},
            headers=self.auth_header,
        )
        self.assertEqual(response.status_code, 400)
        self.assertFalse((Path(self.temp_dir.name).parent / "outside.txt").exists())

    def test_writes_authenticated_relative_path(self):
        response = self.client.post(
            "/form",
            data={"filepath": "experiment/S001/results.txt", "data": "saved"},
            headers=self.auth_header,
        )
        self.assertEqual(response.status_code, 200)
        self.assertEqual(
            (Path(self.temp_dir.name) / "experiment" / "S001" / "results.txt").read_text(),
            "saved",
        )

    def test_requires_data_field(self):
        response = self.client.post(
            "/form",
            data={"filepath": "results.txt"},
            headers=self.auth_header,
        )
        self.assertEqual(response.status_code, 400)


if __name__ == "__main__":
    unittest.main()

`webserver_example.py` is a local sample webserver, written in Python using the Flask framework. It can be used in conjunction with the [HTTP POST Data Handler](https://github.com/immersivecognition/unity-experiment-framework/wiki/HTTP-POST-setup). It accepts incoming POST requests, and writes the data contained in the form to a file. It uses Basic HTTP authentication.

To use it, you need some packages:

```
python -m pip install -r requirements.txt
```

Before starting it, set `UXF_USERNAME`, `UXF_PASSWORD`, and (when needed)
`UXF_OUTPUT_DIR`. `UXF_ALLOWED_ORIGINS` is a comma-separated list of allowed
browser origins; the default `*` is convenient for local experiments only.
The server rejects absolute and traversal paths and keeps every write below the
configured output directory. Use HTTPS, a real secret-management system and a
restricted origin list before adapting this sample for a deployed service.

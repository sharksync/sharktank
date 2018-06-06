module.exports = function (grunt) {

    // args are passed "postBuild:v1.1.1", get the final version number from it
    var buildNumber = process.argv[2].substring(10);

    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),

        cacheBust: {
            indexCacheBust: {
                options: {
                    deleteOriginals: true,
                    assets: ['dist/*.js', 'dist/*.css'],
                    baseDir: 'SharkSync.Web/bin/Release/netcoreapp2.0/publish/wwwroot/'
                },
                src: ['SharkSync.Web/bin/Release/netcoreapp2.0/publish/wwwroot/index.html']
            }
        },

        'string-replace': {
            version: {
                files: {
                    'cloudformation.yaml': 'cloudformation.yaml',
                },
                options: {
                    replacements: [
                        {
                            pattern: 'Default: v0.0.0',
                            replacement: 'Default: ' + buildNumber
                        }
                    ]
                }
            }
        }

    });

    // Plugins used
    grunt.loadNpmTasks('grunt-cache-bust');
    grunt.loadNpmTasks('grunt-string-replace');

    grunt.registerTask('postBuild', ['cacheBust:indexCacheBust', 'string-replace:version']);
};
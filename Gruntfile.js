module.exports = function (grunt) {

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
                            replacement: 'Default: <%= BUILD_VERSION %>'
                        }
                    ]
                }
            }
        }

    });

    // Plugins used
    grunt.loadNpmTasks('grunt-cache-bust');
    grunt.loadNpmTasks('grunt-string-replace');

    grunt.registerTask('loadconst', 'Load environment variables', function () {
        grunt.config('BUILD_VERSION', process.env.BUILD_VERSION);
    });

    grunt.registerTask('postBuild', ['loadconst', 'cacheBust:indexCacheBust', 'string-replace:version']);
};